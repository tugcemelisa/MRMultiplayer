namespace UnityEngine.XR.Templates.MRTTabletopAssets
{
    public class GameObjectToggle : MonoBehaviour
    {
        [SerializeField] GameObject[] objectsToToggle;

        public void ToggleObjects()
        {
            foreach (var obj in objectsToToggle)
            {
                obj.SetActive(!obj.activeSelf);
            }
        }
    }
}
