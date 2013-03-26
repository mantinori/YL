//Devin's ButtonResponder
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


/// <summary>
/// A delegate for a response to a button being pressed.
/// </summary>
/// <param name="o"></param>
public delegate void ButtonResponse(GameObject o);

/// <summary>
/// A more generalized class for responding to button clicks.
/// </summary>
public class ButtonResponder : MonoBehaviour
{
    /// <summary>
    /// The function that should be called when the button is clicked.
    /// </summary>
    public ButtonResponse response;

    public void OnClick()
    {
        if (response != null) {
            response(gameObject);
        } else {
			NeuroLog.Debug("Response method is null");	
		}
    }
}
