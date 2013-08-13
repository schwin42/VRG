using UnityEngine;
using System.Collections;

public class ScriptSelectedCharacterInfo : MonoBehaviour {
	
	GameObject selectedCharacterInfoDisplay;
	ScriptGameMaster scriptGameMaster;
	
	// Use this for initialization
	void Start () {
		selectedCharacterInfoDisplay = transform.FindChild("SelectedCharacterInfoDisplay").gameObject;
		scriptGameMaster = GameObject.Find ("ControllerGame").GetComponent<ScriptGameMaster>();
	}
	
	// Update is called once per frame
	void Update () {
		if(scriptGameMaster.selectedSheet && scriptGameMaster.charactersInPlay.Count > 0){
			selectedCharacterInfoDisplay.guiText.text = GetCharacterInfo(scriptGameMaster.selectedSheet);
		}
	}
	
			string GetCharacterInfo(ScriptCharacterSheet hotSheet){
			return hotSheet.stringID + 
			"\n Health " + hotSheet.health.ToString() +
			"\n Priority " + hotSheet.priority.ToString() +
			"\n Aim " + hotSheet.accuracy.ToString() +
			"\n Melee " + hotSheet.melee.ToString() +
			"\n Defense " + hotSheet.evasion.ToString() +
			"\n Damage " + hotSheet.damage.ToString();
	}
}
