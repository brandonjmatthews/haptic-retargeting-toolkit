using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HRTK
{
    public class MeshColorSelectionIndicator : SelectionIndicator
    {
        [SerializeField] Color _defaultSelectedColor;
        Color _baseColor;
        Color _lastColor;
        Material _materialCopy;
        

        private void Start()
        {
           Initalize();
        }
        
        public override void Initalize()
        {
            _materialCopy = Instantiate(GetComponent<MeshRenderer>().material);
            GetComponent<MeshRenderer>().material = _materialCopy;
            _baseColor = _materialCopy.color;
        }

        public override void OnSelected(Color selectedColor)
        {
            if (_materialCopy == null) return;
            _materialCopy.color = selectedColor;        
        }

        public override void OnSelected()
        {
              if (_materialCopy == null) return;
            _materialCopy.color = _defaultSelectedColor;
        }

        public override void OnDeselected()
        {
            if (_materialCopy == null) return;
            _materialCopy.color = _baseColor;
        }

        public void OverrideColor(Color color) {
            if (_materialCopy == null) return;
            _materialCopy.color = color;
        }

        public void ResetColor() {
            if (_materialCopy == null) return;
            _materialCopy.color = _baseColor;
        }
    }
}
