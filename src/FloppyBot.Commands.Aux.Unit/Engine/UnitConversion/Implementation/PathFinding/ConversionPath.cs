namespace FloppyBot.Commands.Aux.Unit.Engine.UnitConversion.Implementation.PathFinding;

public class ConversionPath : Stack<ConversionNode>
{
    public override string ToString()
        => string.Join(">",
            this.Select(i => i.Name).Reverse());

    public bool IsSamePath(ConversionPath path)
    {
        if (Count != path.Count) return false;

        var me = ToArray();
        var them = path.ToArray();

        for (var i = 0; i < Count; i++)
        {
            if (me[i] != them[i]) return false;
        }

        return true;
    }
}
