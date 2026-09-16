using System;
[Serializable]
public class DialogueActionOpenShop : DialogueAction
{
	public override void Execute()
	{
		if (DialogueManager.Instance.CurrentSpeaker is IShopKeeper shopKeeper)
			ShopManager.Instance.SetupShop(shopKeeper.ShopList);
	}
}
