// OnlineUIButton.cs
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OnlineUIButton : MonoBehaviour
{
    [Header("Références")]
    public Button playOnlineButton;
    public TMP_Text statusText; // Optionnel (UI Text). Si tu utilises TMP, remplace par TMP_Text

    [Header("Contrôleur du flux en ligne")]
    public PlayOnlineController playOnlineController;

    bool _working;

    void Awake()
    {
        if (playOnlineButton != null)
            playOnlineButton.onClick.AddListener(OnPlayOnlineClicked);
    }

    async void OnPlayOnlineClicked()
    {
        if (_working || playOnlineController == null) return;

        _working = true;
        SetInteractable(false);
        SetStatus("Connexion en cours...");

        try
        {
            await playOnlineController.RunOnlineFlowAsync();
            SetStatus("Connecté !");
        }
        catch (System.Exception ex)
        {
            SetStatus("Erreur : " + ex.Message);
            Debug.LogError("[OnlineUIButton] " + ex);
        }
        finally
        {
            // Tu peux laisser le bouton désactivé une fois connecté, à toi de voir.
            // Ici, on le réactive pour pouvoir retenter (pratique en dev).
            SetInteractable(true);
            _working = false;
        }
    }

    void SetInteractable(bool v)
    {
        if (playOnlineButton != null) playOnlineButton.interactable = v;
    }

    void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }
}
