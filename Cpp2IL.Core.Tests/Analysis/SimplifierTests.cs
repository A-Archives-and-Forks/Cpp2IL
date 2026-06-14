using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Cpp2IL.Core.Analysis;
using Cpp2IL.Core.Graphs;
using Cpp2IL.Core.ISIL;
using Cpp2IL.Core.Model.Contexts;

namespace Cpp2IL.Core.Tests.Analysis;

public class SimplifierTests
{
    private static MethodAnalysisContext CreateMethod(ISILControlFlowGraph graph, params LocalVariable[] locals)
    {
        var method = (MethodAnalysisContext)RuntimeHelpers.GetUninitializedObject(typeof(MethodAnalysisContext));
        method.ControlFlowGraph = graph;
        method.Locals = locals.ToList();
        method.ParameterLocals = [];
        return method;
    }

    [Test]
    public void DoesNotInlineAcrossJoinWhenLocalHasMultipleDefinitions()
    {
        var x  = new LocalVariable("x", new Register(null, "x"));
        var cond = new LocalVariable("cond", new Register(null, "cond"));
        var selected = new LocalVariable("selected", new Register(null, "selected"));
        var oddText = new LocalVariable("oddText", new Register(null, "oddText"));
        var evenText = new LocalVariable("evenText", new Register(null, "evenText"));

        var instructions = new List<Instruction>
        {
            new(0, OpCode.CheckNotEqual, cond, x, 1),
            new(1, OpCode.ConditionalJump, 5, cond),
            new(2, OpCode.Move, oddText, "Odd second"),
            new(3, OpCode.Move, selected, oddText),
            new(4, OpCode.Jump, 8),
            new(5, OpCode.Move, evenText, "Even second"),
            new(6, OpCode.Move, selected, evenText),
            new(7, OpCode.Jump, 8),
            new(8, OpCode.CallVoid, "Console.WriteLine", selected, 0),
            new(9, OpCode.Return),
        };

        foreach (var instruction in instructions)
        {
            if (instruction.OpCode is not (OpCode.Jump or OpCode.ConditionalJump))
                continue;

            instruction.Operands[0] = instructions[(int)instruction.Operands[0]];
        }

        var graph = new ISILControlFlowGraph(instructions);
        var method = CreateMethod(graph, cond, selected, oddText, evenText);

        Simplifier.Simplify(method);

        var live = graph.Blocks.SelectMany(b => b.Instructions).ToList();
        var selectedDefinitions = live.Where(i => i.OpCode == OpCode.Move && ReferenceEquals(i.Destination, selected)).ToList();
        Assert.That(selectedDefinitions.Count, Is.EqualTo(2), "both branch assignments to selected must remain");

        var writeLineCall = live.Single(i => i.OpCode == OpCode.CallVoid && i.Operands[0] is "Console.WriteLine");
        Assert.That(ReferenceEquals(writeLineCall.Operands[1], selected), Is.True,
            "join-point call must keep the selected local, not a branch-specific constant");
    }
}
