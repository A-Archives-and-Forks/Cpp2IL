using LibCpp2IL.BinaryStructures;

namespace LibCpp2IL.Metadata;

public class Il2CppParameterDefaultValue : ReadableClass
{
    public int parameterIndex;
    public Il2CppVariableWidthIndex<Il2CppType> typeIndex;
    public int dataIndex;

    public object? ContainedDefaultValue => LibCpp2ILUtils.GetDefaultValue(dataIndex, typeIndex);

    public override void Read(ClassReadingBinaryReader reader)
    {
        parameterIndex = reader.ReadInt32();
        typeIndex = Il2CppVariableWidthIndex<Il2CppType>.Read(reader);
        dataIndex = reader.ReadInt32();
    }
}
