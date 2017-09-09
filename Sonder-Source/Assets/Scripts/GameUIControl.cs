using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class ItemFrame {
	[HideInInspector] public RectTransform rectTransform;

	[HideInInspector] public Image icon;
	
	[HideInInspector] public Sprite iconOriginal;

	[HideInInspector] public Image frame;

	[HideInInspector] public Text itemName;

	RectTransform primaryStat;
	RectTransform secondaryStat;
	Image primaryImage;
	Image secondaryImage;
	float originalWidth = 0;


	public void SetupIcons(string nameInObject, Transform objectInScene) {
		
		rectTransform = objectInScene.Find("TopRight/" + nameInObject).gameObject.GetComponent<RectTransform> ();
		icon = objectInScene.Find("TopRight/"+nameInObject+"/Icon").gameObject.GetComponent<Image> ();
		iconOriginal = icon.sprite;
		frame = rectTransform.GetComponent<Image> ();
		itemName = rectTransform.Find ("Text").GetComponent<Text>();

		primaryStat = (RectTransform) rectTransform.Find ("Primary");
		secondaryStat = (RectTransform) rectTransform.Find ("Secondary");
		primaryImage = primaryStat.GetComponent<Image> ();
		secondaryImage = secondaryStat.GetComponent<Image> ();
		originalWidth = primaryStat.sizeDelta.x;
	}

	public void SetFrame(ItemInstance i) {
		if (!i.isSetup() || i.GetItem().icon == null) {
			icon.sprite = iconOriginal;
		} else {
			icon.sprite = i.GetItem().icon;
		}
		if (i.isSetup()) {
			frame.color = i.GetHalfQualityColor ();
			int netPrimary = i.GetNetPrimary();
			primaryStat.sizeDelta = new Vector2 (Mathf.Max(1,Mathf.Abs(netPrimary)) * originalWidth , primaryStat.sizeDelta.y);
			if (netPrimary > 0)
				primaryImage.color = Color.green;
			else if (netPrimary < 0)
				primaryImage.color = Color.red;
			else
				primaryImage.color = Color.white;

			int netSecondary = i.GetNetSecondary();
			secondaryStat.sizeDelta = new Vector2 (Mathf.Max(1,Mathf.Abs(netSecondary)) * originalWidth , secondaryStat.sizeDelta.y);
			if (netSecondary > 0)
				secondaryImage.color = Color.green;
			else if (netSecondary < 0)
				secondaryImage.color = Color.red;
			else
				secondaryImage.color = Color.white;

			itemName.text = i.GetFullName();
			itemName.color = i.GetLightQualityColor();
		}
	}

	public void CopyIcons (ItemFrame from) {
		primaryImage.sprite = from.primaryImage.sprite;
		secondaryImage.sprite = from.secondaryImage.sprite;
	}


}

public class GameUIControl : MonoBehaviour {
	
	RectTransform healthUI;
	RectTransform healthHolderUI;
	RectTransform energyUI;
	RectTransform energyHolderUI;
	Image levelPercent;
	LevelTextSetter levelTextSetter;

	Animator wingAnim;
	Image wingOutline;
	Image wingFillL;
	Image wingFillR;

	CanvasGroup invGroup;

	ItemFrame weaponFrame;
	ItemFrame armorFrame;
	ItemFrame artifactFrame;
	ItemFrame pickupFrame;
	RectTransform invSelector;

	List<Transform> attachmentPoints;
	

	[HideInInspector] public GameObject mainPlayer;
	PlayerMain playerMain;
	ItemDatabase itemDatabase;

	public float energyBaseH;
	public float holderBaseH;
	public float barRatioH;

	public Color baseFrameColor;

	public List<UIRollIcon> rollIcons;

	public List<UIAbility> abilityIcons;

	// Use this for initialization
	void Start () {
		playerMain = mainPlayer.GetComponent<PlayerMain> ();

		healthUI = transform.Find ("BottomLeft/HealthBar/EnergyMask/Energy").gameObject.GetComponent<RectTransform> ();
		healthHolderUI = transform.Find ("BottomLeft/HealthBar/HolderMask/Holder").gameObject.GetComponent<RectTransform> ();

		energyUI = transform.Find ("BottomLeft/EnergyBar/EnergyMask/Energy").gameObject.GetComponent<RectTransform> ();
		energyHolderUI = transform.Find ("BottomLeft/EnergyBar/HolderMask/Holder").gameObject.GetComponent<RectTransform> ();

		levelPercent = transform.Find ("BottomLeft/Level/Green").gameObject.GetComponent<Image> (); 
		levelTextSetter = transform.Find ("BottomLeft/Level").gameObject.GetComponent<LevelTextSetter> ();

		wingAnim = transform.Find ("Top/Wings").gameObject.GetComponent<Animator> ();
		wingOutline = transform.Find ("Top/Wings/Outline").gameObject.GetComponent<Image> (); 
		wingFillL = transform.Find ("Top/Wings/FillL").gameObject.GetComponent<Image> (); 
		wingFillR = transform.Find ("Top/Wings/FillR").gameObject.GetComponent<Image> ();

		invGroup = transform.Find("TopRight").gameObject.GetComponent<CanvasGroup> ();

		weaponFrame = new ItemFrame ();
		armorFrame = new ItemFrame ();
		artifactFrame = new ItemFrame ();
		pickupFrame = new ItemFrame ();
		
		weaponFrame.SetupIcons ("FrameWeapon", transform);
		armorFrame.SetupIcons ("FrameArmor", transform);
		artifactFrame.SetupIcons ("FrameArtifact", transform);
		pickupFrame.SetupIcons ("FramePickup", transform);

		invSelector = transform.Find ("TopRight/Selector").gameObject.GetComponent<RectTransform> ();

	
	}
	
	// Update is called once per frame
	void LateUpdate () {
		SetEnergyBarSizes ();
		levelPercent.fillAmount = playerMain.experience / playerMain.RequiredExpTotal (); 
		levelTextSetter.SetLevel (playerMain.level);
		SetGlideUI ();
		SetInventorySelector ();
		SetInventoryImages ();
		SetRollIcons ();
		UpdateAbilityIcons ();

		DEBUGTurnAbilitiesOn ();


	
	}
	void SetEnergyBarSizes() {
		healthUI.sizeDelta = new Vector2 (healthUI.sizeDelta.x, GetEnergyHeight(false, playerMain.health)); 
		healthHolderUI.sizeDelta = new Vector2 (healthHolderUI.sizeDelta.x, GetEnergyHeight(true, playerMain.maxHealth)); 
		energyUI.sizeDelta = new Vector2 (energyUI.sizeDelta.x, GetEnergyHeight(false, playerMain.energy)); 
		energyHolderUI.sizeDelta = new Vector2 (energyHolderUI.sizeDelta.x, GetEnergyHeight(true, playerMain.maxEnergy)); 
	}

	void SetGlideUI() {
		//if (playerMain.movementController.GetMoveState () == MoveState.gliding)
		//	wingAnim.speed = 1;
		//	//wingAnim.SetFloat("Gliding", 1);
		//else 
		//	wingAnim.speed = -1;


		float percent = playerMain.movementController.glideStamina / playerMain.maxGlideStamina;
		float alpha = 1 - 10*Mathf.Max (percent-.9f, 0);
		wingFillL.fillAmount = playerMain.movementController.glideStamina / playerMain.maxGlideStamina;
		wingFillR.fillAmount = playerMain.movementController.glideStamina / playerMain.maxGlideStamina;
		wingFillL.color = new Color(1,1,1,alpha);
		wingFillR.color = new Color(1,1,1,alpha);
		wingOutline.color = new Color(0,0,0,alpha);

	}

	void SetInventorySelector() {

		if (!playerMain.GetItemOnTopOf ().isSetup()) {
			invSelector.position = new Vector3 (1000000, 1000000, 0);
			invGroup.alpha = Mathf.Clamp(invGroup.alpha - 3*Time.deltaTime,0, 1);
		} else {
			invGroup.alpha = Mathf.Clamp(invGroup.alpha + 3*Time.deltaTime,0, 1);
			if (playerMain.GetItemOnTopOf().GetItem().type == ItemType.weapon)
				invSelector.localPosition = weaponFrame.rectTransform.localPosition;
			else if (playerMain.GetItemOnTopOf().GetItem().type == ItemType.armor)
				invSelector.localPosition = armorFrame.rectTransform.localPosition;
			else if (playerMain.GetItemOnTopOf().GetItem().type == ItemType.artifact)
				invSelector.localPosition = artifactFrame.rectTransform.localPosition;
		}
	}

	void SetInventoryImages() {

		weaponFrame.SetFrame(playerMain.GetWeaponInst ());
		armorFrame.SetFrame(playerMain.GetArmorInst ());
		artifactFrame.SetFrame(playerMain.GetArtifactInst ());
		pickupFrame.SetFrame(playerMain.GetItemOnTopOf ());
		if (playerMain.GetItemOnTopOf ().isSetup()) {
			if (playerMain.GetItemOnTopOf().GetItem().type == ItemType.weapon) {
				pickupFrame.CopyIcons(weaponFrame);
			} else if (playerMain.GetItemOnTopOf().GetItem().type == ItemType.armor) {
				pickupFrame.CopyIcons(armorFrame);
			} else if (playerMain.GetItemOnTopOf().GetItem().type == ItemType.artifact) {
				pickupFrame.CopyIcons(artifactFrame);
			}
		}

		//weaponFrame.frame.color = playerMain.GetWeapon ().GetQualityColor () / 2f;
	}

	float GetEnergyHeight(bool holder, float value) {
		if (holder) {
			return holderBaseH + barRatioH*value;
		} else {
			return energyBaseH + barRatioH*value;
		}
		
	}

	void SetRollIcons() {
		int maximum = playerMain.movementController.maxInvincRolls;
		float current = playerMain.movementController.invincRollsRemaining;
		for (int index = 0; index < rollIcons.Count; index ++) {
			rollIcons[index].gameObject.SetActive(index < maximum);
			rollIcons[index].SetVariable(current - index);
		}
		//playerMain.movementController.invincRollsRemaining
	}

	void UpdateAbilityIcons() {
		UpdateSingleAbilityIcon (playerMain.actionsController.GetWeaponAction(), 0);
		UpdateSingleAbilityIcon (playerMain.actionsController.GetArmorAction(), 1);
		UpdateSingleAbilityIcon (playerMain.actionsController.GetArtifactAction(), 2);
	}

	void UpdateSingleAbilityIcon(Action action, int index) {
		if (action != null) {
			abilityIcons[index].UpdateCooldown(action.GetRemainingCooldown(), action.GetMaxCooldown());
		}
	}







	void DEBUGTurnAbilitiesOn() {
		if (!Input.GetKey(KeyCode.LeftShift) && Input.GetKey (KeyCode.LeftAlt) && Input.GetKeyDown (KeyCode.Alpha1)) {
			abilityIcons[0].gameObject.SetActive(!abilityIcons[0].gameObject.activeSelf); 
		}
		if (!Input.GetKey(KeyCode.LeftShift) && Input.GetKey (KeyCode.LeftAlt) && Input.GetKeyDown (KeyCode.Alpha2)) {
			abilityIcons[1].gameObject.SetActive(!abilityIcons[1].gameObject.activeSelf); 
		}
		if (!Input.GetKey(KeyCode.LeftShift) && Input.GetKey (KeyCode.LeftAlt) && Input.GetKeyDown (KeyCode.Alpha3)) {
			abilityIcons[2].gameObject.SetActive(!abilityIcons[2].gameObject.activeSelf); 
		}
		if (!Input.GetKey(KeyCode.LeftShift) && Input.GetKey (KeyCode.LeftAlt) && Input.GetKeyDown (KeyCode.BackQuote)) {
			abilityIcons[3].gameObject.SetActive(!abilityIcons[3].gameObject.activeSelf); 
		}
		if (!Input.GetKey(KeyCode.LeftShift) && Input.GetKey (KeyCode.LeftAlt) && Input.GetKeyDown (KeyCode.Alpha4)) {
			abilityIcons[4].gameObject.SetActive(!abilityIcons[4].gameObject.activeSelf); 
		}
		if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey (KeyCode.LeftAlt) && Input.GetKeyDown (KeyCode.Alpha1)) {
			abilityIcons[5].gameObject.SetActive(!abilityIcons[5].gameObject.activeSelf); 
		}
		if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey (KeyCode.LeftAlt) && Input.GetKeyDown (KeyCode.Alpha2)) {
			abilityIcons[6].gameObject.SetActive(!abilityIcons[6].gameObject.activeSelf); 
		}
		if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey (KeyCode.LeftAlt) && Input.GetKeyDown (KeyCode.Alpha3)) {
			abilityIcons[7].gameObject.SetActive(!abilityIcons[7].gameObject.activeSelf); 
		}
		if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey (KeyCode.LeftAlt) && Input.GetKeyDown (KeyCode.Alpha4)) {
			abilityIcons[8].gameObject.SetActive(!abilityIcons[8].gameObject.activeSelf); 
		}
		//TODO continue here
	}

	public void UpdateAbilityIconFromItem(ItemType itemType) {
		if (itemType == ItemType.weapon) {
			abilityIcons[0].UpdateIcon(playerMain.GetWeapon().action);
		}
		if (itemType == ItemType.armor) {
			abilityIcons[1].UpdateIcon(playerMain.GetArmor().action);
		}
		if (itemType == ItemType.artifact) {
			abilityIcons[2].UpdateIcon(playerMain.GetArtifact().action);
		}
	}




	
}
