using Schemata.Resource.Foundation.Filter.Terms;

namespace Schemata.Resource.Foundation.Filter.Operations;

public class LessThan : IBinary
{
    public const char Char = '<';

    #region IBinary Members

    public bool IsConstant => false;

    #endregion

    public override string ToString() {
        return $"{Char}";
    }
}
