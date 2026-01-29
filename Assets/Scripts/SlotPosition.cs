using UnityEngine;

public class SlotPosition : MonoBehaviour
{
    [SerializeField] private int gridRow;
    [SerializeField] private int gridColumn;

    public int GridRow => gridRow;
    public int GridColumn => gridColumn;

    public void SetGridPosition(int row, int col)
    {
        gridRow = row;
        gridColumn = col;
    }

    public Vector2Int GetGridPosition()
    {
        return new Vector2Int(gridRow, gridColumn);
    }

    // Utile pour déboguer en inspecteur
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
}
