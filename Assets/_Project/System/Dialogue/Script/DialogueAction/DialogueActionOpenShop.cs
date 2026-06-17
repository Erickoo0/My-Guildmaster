using UnityEngine;

[System.Serializable]
public class DialogueActionOpenShop : DialogueAction
{
    public override void Execute()
    {
        ShopManager.Instance.SetupShop(DialogueManager.Instance.CurrentSpeaker.ShopList);
    }
}
