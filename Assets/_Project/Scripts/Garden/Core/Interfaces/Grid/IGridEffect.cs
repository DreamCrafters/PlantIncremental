public interface IGridEffect
{
    void Apply(GridCell center, GridCell[] neighbors);
}