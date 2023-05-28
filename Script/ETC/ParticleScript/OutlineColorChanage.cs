using UnityEngine;

public class OutlineColorChanage : MonoBehaviour {

    public Renderer myRenderer = null;

    public Color sColor = Color.white;
    public Color eColor = Color.white;

    private void Awake()
    {
        myRenderer = this.gameObject.GetComponent<Renderer>();
    }

    private void Update()
    {
        myRenderer.material.color = Color.Lerp(sColor, eColor, Mathf.PingPong(Time.time, 3));
    }
}
