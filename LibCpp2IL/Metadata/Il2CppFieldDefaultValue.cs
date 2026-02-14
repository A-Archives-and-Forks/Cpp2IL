using LibCpp2IL.BinaryStructures;

namespace LibCpp2IL.Metadata;

public class Il2CppFieldDefaultValue : ReadableClass
{
    public int fieldIndex;
    public Il2CppVariableWidthIndex<Il2CppType> typeIndex;
    public int dataIndex;

    public object? Value => dataIndex <= 0 ? null : LibCpp2ILUtils.GetDefaultValue(dataIndex, typeIndex);

    public override void Read(ClassReadingBinaryReader reader)
    {
        fieldIndex = reader.ReadInt32();
        typeIndex = Il2CppVariableWidthIndex<Il2CppType>.Read(reader);
        dataIndex = reader.ReadInt32();
    }
}
