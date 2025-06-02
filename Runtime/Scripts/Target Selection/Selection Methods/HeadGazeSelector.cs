/*
 * HRTK: HeadGazeSelector.cs
 *
 * Copyright (c) 2019 Brandon Matthews
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HRTK
{
    public class HeadGazeSelector : RayDwellSelector
    {

        protected override void Update()
        {
            base.Update();
            if (SelectorEnabled && SelectionEnabled)
            {
                Ray centreRay = ScreenCentreRay();
                UpdateRayTarget(centreRay);
            }
        }

        public Ray ScreenCentreRay()
        {
            Vector2 screenCentre;
            screenCentre.x = Camera.main.pixelWidth / 2;
            screenCentre.y = Camera.main.pixelHeight / 2;
            Ray ray = _manager.Head.GetComponentInChildren<Camera>().ScreenPointToRay(screenCentre);
            return ray;
        }
    }
}
