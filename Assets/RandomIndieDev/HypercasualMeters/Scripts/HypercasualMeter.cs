using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HypercasualMeter : MonoBehaviour
{

    [Header("Settings")]
    [Tooltip("Change the speed the pointer moves at")]
    public float pointerSpeed;
    
    [Tooltip("Displays")]
    public string defaultDisplayValue;

    public bool startOnPlay;
    
    [Header("Values")]
    public float minPossibleValue;
    public float maxPossibleValue;

    [Header("References")]
    public Animator _animator;
    public TextMeshProUGUI _valueText;

    [SerializeField]
    [Tooltip("Leave this alone, using this to know where the value is currently on the slider")]
    [Range(0, 100)]
    private float _animationSlider;

    void Start()
    {
        // Sets the configured speed to the animation
        _animator.SetFloat("PointerMoveSpeed", pointerSpeed);
        
        // Sets user inputed default text
        _valueText.text = defaultDisplayValue;
        
        if (startOnPlay)
            _animator.SetTrigger("TriggerPointerMove");
    }
    private void UpdateValue(float value)
    {
        _valueText.text = value + "";
    }

    //                            PUBLIC FUNCTIONS
    
    // Gets the current value (0-100) where ever the pointer is at the time of calling this function
    public float GetCurrentValue()
    {
        var sliderValue = Mathf.Round(_animationSlider);
        var diff = maxPossibleValue - minPossibleValue;

        var finalValue = (diff * sliderValue / 100) + minPossibleValue;
        
        finalValue = Mathf.Round(finalValue);
        
        UpdateValue(finalValue);
        
        return sliderValue;

    }
    
    // Start / Stop the meter animation from playing
    public void TriggerMeter()
    {
        _animator.SetTrigger("TriggerPointerMove");
    }
}
