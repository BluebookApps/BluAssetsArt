using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MeterUsingScript : MonoBehaviour
{
    [Header("Meter References")] 
    public HypercasualMeter curvedMeter;
    public HypercasualMeter straightMeter;

    [Header("UI References")] 
    public TextMeshProUGUI valueDisplayText;
    
    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            curvedMeter.TriggerMeter();
            straightMeter.TriggerMeter();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            var curvedMeterValue = curvedMeter.GetCurrentValue();
            var straightMeterValue = straightMeter.GetCurrentValue();

            var text = "Curved Meter Value: " + curvedMeterValue + "\n" + "Straight Meter Value: " + straightMeterValue;

            valueDisplayText.text = text;
        }
    }
}
