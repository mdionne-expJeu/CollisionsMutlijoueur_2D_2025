using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatchmakingBouton : MonoBehaviour
{
    [Header("Références")]
    public TMP_Text statusText; // Référence au texte qui affichera l'état de la connexion

    [Header("Contrôleur du flux en ligne")]
    public PlayOnlineController playOnlineController; //Référence au script principal pour la gestion du matchmaker

    bool _working;


    /* 
    Fonction asynchrone publique qui doit être appelée par le bouton qui lance la connexion
    */
    public async void OnPlayOnlineClicked()
    {
        if (_working || playOnlineController == null) return;

        _working = true;
        SetStatus("Connexion en cours...");

        try
        {
            //Activation de la fonction principale du script PlayOnlineController
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
            _working = false;
        }
    }

    

    void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }
}
