using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.github.lhervier.ksp.shared.ugui
{
    /// <summary>
    /// Gestionnaire léger de survol/clic pour un élément situé DANS un ScrollRect.
    ///
    /// On n'implémente ni IScrollHandler ni IDragHandler (contrairement à EventTrigger qui implémente
    /// tout) : molette et glisser remontent donc au ScrollRect parent. On implémente en revanche
    /// IPointerDownHandler (vide) en plus de IPointerClickHandler : c'est la présence d'un down-handler
    /// qui fait que l'EventSystem « réserve » la pression sur cet objet et déclenche correctement
    /// OnPointerClick face à l'arbitrage clic/drag du ScrollRect (même mécanisme qu'un Button standard).
    /// </summary>
    public class PointerHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler
    {
        public Action OnEnter;
        public Action OnExit;
        public Action OnClick;

        public void OnPointerEnter(PointerEventData eventData) { OnEnter?.Invoke(); }
        public void OnPointerExit(PointerEventData eventData) { OnExit?.Invoke(); }

        // Vide : sert uniquement à « réserver » la pression pour que OnPointerClick parte correctement.
        public void OnPointerDown(PointerEventData eventData) { }

        public void OnPointerClick(PointerEventData eventData) { OnClick?.Invoke(); }
    }
}
