using UnityEngine;
using TMPro;

namespace com.github.lhervier.ksp.shared.ugui.textfield
{
    /// <summary>
    /// Pilote un champ de saisie partagé (mono- ou multi-ligne). Encapsule entièrement le verrou clavier
    /// KSP : le clavier du jeu est verrouillé tant que le champ a le focus, et déverrouillé au blur (ou à
    /// la destruction du champ). Expose la valeur via l'événement OnValueChanged et l'API Get/SetText.
    /// </summary>
    public class TextFieldController : MonoBehaviour
    {
        /// <summary>Émis à chaque modification du texte (saisie clavier ou SetText).</summary>
        public EventData<string> OnValueChanged = new EventData<string>("KSPShared.UGUI.TextField.OnValueChanged");

        /// <summary>Émis à la validation (Entrée en mono-ligne, ou perte de focus).</summary>
        public EventData<string> OnEndEdit = new EventData<string>("KSPShared.UGUI.TextField.OnEndEdit");

        // Identifiant de verrou unique à cette instance, pour éviter toute collision entre champs.
        private string _lockId;
        private string LockId => _lockId ?? (_lockId = "KSPShared.TextField." + GetInstanceID());

        // Vrai pendant SetText : l'affectation programmatique de .text déclenche onValueChanged de façon
        // synchrone, ce drapeau permet de ne pas répercuter ce changement « non utilisateur » vers OnValueChanged.
        private bool _suppressNotify = false;

        // ============================================
        // Life cycle
        // ============================================

        private TMP_InputField _input;
        public TextFieldController Input(TMP_InputField input)
        {
            this._input = input;
            return this;
        }

        public void Start()
        {
            if (_input != null)
            {
                _input.onValueChanged.AddListener(OnInputValueChanged);
                _input.onEndEdit.AddListener(OnInputEndEdit);
                _input.onSelect.AddListener(OnInputSelected);
                _input.onDeselect.AddListener(OnInputDeselected);
            }
        }

        public void OnDestroy()
        {
            if (_input != null)
            {
                _input.onValueChanged.RemoveListener(OnInputValueChanged);
                _input.onEndEdit.RemoveListener(OnInputEndEdit);
                _input.onSelect.RemoveListener(OnInputSelected);
                _input.onDeselect.RemoveListener(OnInputDeselected);
            }
            // Filet de sécurité : si le champ est détruit alors qu'il avait le focus, le verrou resterait
            // sinon actif et figerait les commandes du jeu.
            InputLockManager.RemoveControlLock(LockId);
        }

        // ============================================
        // Methods bound to events
        // ============================================

        private void OnInputValueChanged(string value)
        {
            if (_suppressNotify) return;
            OnValueChanged.Fire(value);
        }

        private void OnInputEndEdit(string value)
        {
            OnEndEdit.Fire(value);
        }

        private void OnInputSelected(string _)
        {
            InputLockManager.SetControlLock(ControlTypes.All, LockId);
        }

        private void OnInputDeselected(string _)
        {
            InputLockManager.RemoveControlLock(LockId);
        }

        // ============================================
        // Public API
        // ============================================

        public string GetText()
        {
            return _input != null ? _input.text : string.Empty;
        }

        /// <summary>
        /// Affecte le texte. N'émet PAS OnValueChanged (synchronisation depuis le modèle sans boucle
        /// de rétroaction). Utiliser une saisie utilisateur pour déclencher l'événement.
        /// </summary>
        public void SetText(string text)
        {
            if (_input == null) return;
            _suppressNotify = true;
            try
            {
                _input.text = text ?? string.Empty;
            }
            finally
            {
                _suppressNotify = false;
            }
        }
    }
}
