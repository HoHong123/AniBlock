
public class LoadSceneChangeScript : LoadSceneScript
{
    public string ChangeSceneName;

    private void Start()
    {
        if(Manager.Util.XmlManager.S != null)
        {
            if(Manager.Util.PackageManager.Instance.FC_PACKAGE != Manager.Util.PackageManager.FirstCollection.NumOf)
            {
                moveSceneName = ChangeSceneName;
            }
        }
    }
}
