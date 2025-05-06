using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HRTK {
    public class Reticle : MonoBehaviour
    {

        public Image centerImage;
        public Image fillImage;
        public Image invalidImage;

        public void SetFillAmount(float amount) {
            if (fillImage != null) fillImage.fillAmount = amount;
        }

        public void ToggleInvalidIndicator(bool isInvalid) {
            centerImage.gameObject.SetActive(!isInvalid);
            fillImage.gameObject.SetActive(!isInvalid);
            invalidImage.gameObject.SetActive(isInvalid);
        }
    }
}