using UnityEngine;

namespace NG
{
    public class KeyPressExample : MonoBehaviour
    {
        [SerializeField]
        ScreenshotHelper screenshotHelper;

        [SerializeField]
        KeyCode keyToHold = KeyCode.LeftShift;

        [SerializeField]
        KeyCode keyToPress = KeyCode.S;

        void Reset()
        {
            if (screenshotHelper == null)
                screenshotHelper = FindObjectOfType<ScreenshotHelper>();
        }

        void Update()
        {
            if (keyToHold == KeyCode.None && keyToPress != KeyCode.None)
            {
                if (Input.GetKeyDown(keyToPress))
                    screenshotHelper.GetScreenShots();
            }
            else if (keyToHold != KeyCode.None && keyToPress != KeyCode.None)
            {
                if (Input.GetKey(keyToHold) && Input.GetKeyDown(keyToPress))
                    screenshotHelper.GetScreenShots();
            }
        }
    }
}