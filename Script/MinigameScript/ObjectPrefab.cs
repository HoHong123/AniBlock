using System.Collections;
using UnityEngine;

public class ObjectPrefab : MonoBehaviour
{
    public Transform[,] TileArrary = null;

    public SpriteRenderer _spr = null;

    private void Awake()
    {
        InitTile();
    }

    private void InitTile()
    {
        TileArrary = new Transform[8, 8];

        float sss = -8.27f;
        float _x = sss;
        float _y = +8.27f;
        float _offset = 2.365f;

        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                GameObject _tile = Instantiate(new GameObject(), this.gameObject.transform);
                _tile.transform.localPosition = new Vector3(_x, _y, 0.001f);
                _tile.name = y + "_" + x;

                _x += _offset;

                TileArrary[y, x] = _tile.transform;
            }

            _x = sss;
            _y -= _offset;
        }
    }

    public void ChangePattern(Sprite _sprite)
    {
        _spr.sprite = _sprite;
    }
}
