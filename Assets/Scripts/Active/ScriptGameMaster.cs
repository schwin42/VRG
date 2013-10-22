using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class ScriptGameMaster : MonoBehaviour {
	

	
	/*NOTES
	 * 
	 * 
	 * 
	 * */
	
	
	//Phases - Actions resolve in execution phase; players issue orders in command phase
	public enum Phase 
	{
		Execution,
		Command
	}
	
	//Modes - Play alternates between movement and engagement modes while in execution phase
	public enum Mode
	{
		Movement,
		Command,
		Engagement
	}

	public Phase gamePhase;
	public Mode gameMode;
	
	//Modes
	public bool movementMode = false;
	public bool engagementMode = false;
	
	//Interface
	GameObject interfaceMain;
	ScriptInterface scriptInterface;
	public string inputButtonName = "";
	ScriptCycleDisplay scriptCycleDisplay;
	public GameObject damageDisplay;
	public float damageDisplayDepth = -1;
	
	//Characters
	public GameObject characterTemplate;
	public List<ScriptCharacterSheet> charactersInPlay = new List<ScriptCharacterSheet>();
	public int nextCharacterID = 0;
	public List<ScriptCharacterSheet> activeCharacters = new List<ScriptCharacterSheet>();
	public int spawn00Time = -1;
	public int spawn01Time = -1;
	public GameObject conCharacter;
	public ScriptCharacterSheet selectedSheet;
	public ScriptCharacterSheet opposingSheet;
	
	//Items
	public List<Item> itemsInPlay = new List<Item>();
	
	//Space
	public Transform spawn00;
	public Transform spawn01;
	
	//Time
	public int cycle = -1;
	public float cycleTimer;
	public float cycleLength = 10;
	public float timerConstant = 1;
	
	//Mechanics
	
		//Tactics
	//public int aggressiveFirePriorityBonus = 10;
	
		
	
	//Physics
	ScriptPhysicsController scriptPhysicsController;
	
	//Effects
	//public GameObject energyBall;
	
	//Records
		//Database
	ScriptDatabase scriptDatabase;
		//Names
	List<string> firstNames = new List<string>(new string[] {"Jumbo", "Ham", "Tassik", 
		"Marinn", "Rose", "Joseph", "Dash", "Jaedon", "Argot", "Tau", "Rachel", "Julien", "Lily", "Larry", 
		"Maynard", "Leo", "Ota", "Gulliver", "Megan", "Freck", "Korder", "Lincoln"});
	List<string> lastNames = new List<string>(new string[] {"Baloney", "Jehosephat", "Kayla", 
		"Dillon", "Reynolds", "Wild", "Rendar", "Casio", "Veis", "Ceti", "Vega", "Pavec", "Puncture", 
		"Jello", "Thatcher", "Marshall", "Stockholm", "Retri", "Freck", "Korder", "Lincoln"});
	
	//Debug
	//public GameObject testCharacter;
	//public GameObject[] testArray;
	//public List<GameObject> tempCharactersInPlay = new List<GameObject>();
	
	//Classes
	[System.Serializable]
	public class Result
	{
		public ScriptCharacterSheet actingCharacter = null;
		public ScriptCharacterSheet targetCharacter = null;
		public bool success = false;
		public DamageType damageType = DamageType.None;
		public int damageAmount = -9999;
		public int successNumber = -9999;
		public int hitPercentage = -9999;
		public int roll = -9999;
		public int actingAttack = -9999;
		public int targetDefense = -9999;
		public BodyPart hitLocation = BodyPart.None;
		public Result(ScriptCharacterSheet actingCharacterSheet)
		{

			actingCharacter = actingCharacterSheet;
		}
	}
	
	// Use this for initialization
	void Start () {
		
		
		//Acquire scripts
		interfaceMain = GameObject.Find ("InterfaceMain");
		scriptInterface = interfaceMain.GetComponent<ScriptInterface>();
		scriptCycleDisplay = interfaceMain.transform.FindChild("PanelCycle").GetComponent<ScriptCycleDisplay>();
		scriptPhysicsController = GameObject.Find ("ControllerPhysics").GetComponent<ScriptPhysicsController>();
		scriptDatabase = GetComponent<ScriptDatabase>();
		
		//Acquire controllers
		conCharacter = GameObject.Find ("ConCharacter");
		
		//Register each character object in the scene
		foreach(GameObject character in GameObject.FindGameObjectsWithTag("Character")){
			ScriptCharacterSheet hotSheet = character.GetComponent<ScriptCharacterSheet>();
			if(selectedSheet == null){				
				SetAsSelected(RegisterCharacter(hotSheet));
			} else {
				//ScriptCharacterSheet hotSheet = character.GetComponent<ScriptCharacterSheet>();
				//int output = hotSheet.GetCharacterPriority(0);
				//Debug.Log (output.ToString());
				RegisterCharacter(hotSheet);	
			}
		}
		
		//Spawn a random character on the left and right spawnpoints and give a random item
		GiveCharacterItem(RegisterCharacter(RandomizeCharacterValues(NewCharacter(spawn00))), CreateRandomItem());
		GiveCharacterItem(RegisterCharacter(RandomizeCharacterValues(NewCharacter(spawn01))), CreateRandomItem());
		
		RolloverCycle();
		
		//Debug
		//Debug.Log (charactersInPlay[0].GetComponent<ScriptCharacterSheet>().unequippedItems[0].netStatProfile.attackModifier);
	}
	
	// Update is called once per frame
	void Update () {
		
		
		//if(Input.GetKeyDown(KeyCode.N)){
		//	;
		//	RandomizeCharacterValues(RegisterCharacter(NewCharacter()));
		//}
		
		//if(Input.GetKeyDown(KeyCode.C)){
		//foreach(GameObject character in charactersInPlay)
		//	{
		//	character.transform.FindChild("ObjectCharacterModel").SendMessage("ColorCharacter");	
		//	}
		//}
		
		//if(Input.GetKeyDown(KeyCode.P)){
		//	CreatePlayerCharacter();	
		//}
		if(inputButtonName != ""){
			ButtonHandler();
		}
		
		if(movementMode){
		cycleTimer += Time.deltaTime * timerConstant;
			if(cycleTimer >= cycleLength){
			SetToEngagementMode();
			//RolloverCycle();	
			}
		}
		
		
	}
	
	

	
	//BEGIN FUNCTIONS
	
	//Character Management
	ScriptCharacterSheet NewCharacter(Transform spawnTransform){
	//Create character object at spawn position
		//if(spawnPosition == 0)
		//{
	
			
		//Create character at spawn point
			GameObject hotChar = Instantiate(characterTemplate, spawnTransform.position, spawnTransform.rotation) as GameObject;
			ScriptCharacterSheet hotSheet = hotChar.GetComponent<ScriptCharacterSheet>();
			
			//Place in character container
			hotChar.transform.parent = conCharacter.transform;		
			
		//Assign left character as selected and right as opposing; assign position objective
		if(spawnTransform == spawn00){
			hotSheet.positionObjective = new Vector3(-1.75F, hotChar.transform.position.y, hotChar.transform.position.z);
			selectedSheet = hotSheet;
		} else if(spawnTransform == spawn01){		
			hotSheet.positionObjective = new Vector3(-3.25F, hotChar.transform.position.y, hotChar.transform.position.z);
		opposingSheet = hotSheet;	
		} else {
			Debug.Log ("Invalid spawn position");	
			}
			
			
			
			
			return hotSheet;
		//}
		//else if(spawnPosition == 1)
		//{
		//	GameObject hotChar = Instantiate(characterTemplate, spawn01.position, spawn01.rotation) as GameObject;
		//	ScriptCharacterSheet hotSheet = hotChar.GetComponent<ScriptCharacterSheet>();
		//	hotSheet.positionObjective = new Vector3(-3.25F, hotChar.transform.position.y, hotChar.transform.position.z);
		//	return hotChar;
			
		//}
		//else 
		//{
		//	Debug.Log ("Invalid Spawn Position");
		//	return null;
		
			
		}
		
		//return hotChar;
		//return Instantiate(characterTemplate, spawnTransform.position, spawnTransform.rotation) as GameObject;
			
			//new Vector3(spawnPosition.x + nextCharacterID * 2, spawnPosition.y, spawnPosition.z), transform.rotation) as GameObject;
	
	ScriptCharacterSheet RegisterCharacter(ScriptCharacterSheet hotSheet){
		//ScriptCharacterSheet hotSheet = character.GetComponent<ScriptCharacterSheet>();
		
		scriptPhysicsController.SendMessage("Unragdollify", hotSheet.gameObject);
		
		hotSheet.characterID = nextCharacterID;
		nextCharacterID += 1;
		charactersInPlay.Add (hotSheet);
		hotSheet.stringID = hotSheet.characterID.ToString() + hotSheet.firstName + hotSheet.lastName;
		hotSheet.fullName = hotSheet.stringID;
		
		//Assign object character name
		hotSheet.gameObject.name = hotSheet.stringID;
		
		//Set color
		GameObject hotModel = hotSheet.gameObject.transform.FindChild("ObjectCharacterModel").gameObject;
		hotModel.SendMessage("InitializeModel");
		hotModel.SendMessage("ColorCharacter");
		
		//Set Derived Stats
		hotSheet.headHP = 4;
		hotSheet.bodyHP = 7;
		hotSheet.leftArmHP = 4;
		hotSheet.rightArmHP = 4;
		hotSheet.leftLegHP = 5;
		hotSheet.rightLegHP = 5;
		hotSheet.unarmedDamage = hotSheet.muscle / 2;
		
		return hotSheet;
	}
	
	ScriptCharacterSheet SetAsSelected(ScriptCharacterSheet hotSheet){
				//If first character, assign as selected
		
			selectedSheet = hotSheet;
		return hotSheet;
		
	}
	ScriptCharacterSheet RandomizeCharacterValues(ScriptCharacterSheet hotSheet){
		//ScriptCharacterSheet hotSheet = character.GetComponent<ScriptCharacterSheet>();
		
		//Set name and update game object
		hotSheet.firstName = firstNames[(int)Mathf.Floor(Random.value*firstNames.Count)];
		hotSheet.lastName = lastNames[(int)Mathf.Floor(Random.value*lastNames.Count)];
		hotSheet.fullName = hotSheet.firstName+ " " + hotSheet.lastName;
		hotSheet.stringID = hotSheet.characterID.ToString() + hotSheet.firstName.ToString() + hotSheet.lastName.ToString();
		hotSheet.name = hotSheet.stringID;
		
		//Assign Colors
		hotSheet.primaryColor = GetRandomColor();
		hotSheet.secondaryColor = GetRandomColor();
		
		//Assign Stats
		hotSheet.meat = GetRandom1ToN(10);
		hotSheet.nerve = GetRandom1ToN(10);
		hotSheet.muscle = GetRandom1ToN(10);
		hotSheet.baseAttack = GetRandom1ToN(10);
		hotSheet.baseDefense = GetRandom1ToN(5);
		//hotSheet.unarmedRange = GetRandom1ToN(6);
		
		//Assign Tactics
		//hotSheet.targetReassess = GetRandomBool();
		//if(GetRandomBool()){
		
		//	hotSheet.engageAtRange = true;
		//	hotSheet.engageInMelee = false;
		
		//} else {
		//	hotSheet.engageAtRange = false;
		//	hotSheet.engageInMelee = true;
		//}
		
		//Assign Derived Stats
		
		//hotSheet.priority = hotSheet.focus;
		
		
		
		
		
		//hotSheet.weaponCooldown = GetRandom1ToN(3);
		//hotSheet.weaponRange = 1;
		//resistance
		
		return hotSheet;
		
	}
	
	int GetCharactersInPlayIndex(ScriptCharacterSheet hotSheet){
		for(int i = 0; i < charactersInPlay.Count; i++){
			if(hotSheet == charactersInPlay[i])
			{
			return i;	
			}
		}
		
		Debug.Log ("Character Index not found.");
		return -9999;
	}
	
	void KillCharacter(ScriptCharacterSheet hotSheet){
				
		//Debug.Log (hotSheet.characterID + "killed");
		//Remove dead character from characters in play 
		charactersInPlay.RemoveAt(GetCharactersInPlayIndex(hotSheet));
		//Set character's inPlay to false
		hotSheet.inPlay = false;
		
		//Disable character model's face
		//GameObject hotFace = hotSheet.gameObject.GetComponentInChildren<ScriptModelController>().face;
		//hotFace.SetActive(false);
		
		//transform.Find ("ObjectCharacterObjectCharacterModelheadfacefaceImage").gameObject.SetActive(false);
		
		//Remove character as an valid target
		foreach(ScriptCharacterSheet otherHotSheet in charactersInPlay){
			//ScriptCharacterSheet otherHotSheet = character.GetComponent<ScriptCharacterSheet>();
			if(otherHotSheet.target == hotSheet){
				otherHotSheet.target = null;
			}
		}
		//Log death
		scriptInterface.SendMessage("AddNewLine", hotSheet.fullName + " dies.");
		
		//Death physics
	ScriptCharacterSheet lastAttackerSheet = hotSheet.lastAttacker.GetComponent<ScriptCharacterSheet>();
			scriptPhysicsController.propel = lastAttackerSheet.propel;
		scriptPhysicsController.blowUpHead = lastAttackerSheet.blowUpHead;
			
		
		scriptPhysicsController.SendMessage("ExecuteCharacter", hotSheet.gameObject);
		
		//Set new character spawn time
		if(hotSheet.gameObject.transform.rotation.y == 0){
			
					spawn01Time = cycle + 3;
		} else
		//if(hotSheet.gameObject.transform.rotation.y == 180)
		{
					spawn00Time = cycle + 3;
		}
		//} else {
		//Debug.Log ("Unexpected");	
		//}
}
	
	
	//Progress to next event
	void ResolveEngagement(){
		
		if(engagementMode)
		{
			//Set activeCharacters
			UpdateCharacterValues();
			GetActiveCharacters();
		//Debug.Log (activeCharacters.Count.ToString());
			
			//If there any characters left to act for this Cycle, then execute next action
			if(activeCharacters.Count >= 1)
			{
				//1. Determine character order
				SortActiveCharacters();
				
				//2. Exceute next action in queue
				ExecuteAction(activeCharacters[0]);
				
				//3. If more than one character and characters are tied for priority, both actions resolve before registering new states
				if(activeCharacters.Count > 1 && GetCharacterPriority(activeCharacters[0]) == GetCharacterPriority(activeCharacters[1]))
				{
				ExecuteAction(activeCharacters[1]);	
				}
				
				//4. Kill necessary characters
				CharacterCleanup();
				
				//5. Update character target, destination, stats
				UpdateCharacterValues();
				ResolveEngagement ();
			} 
			else 
			{
				RolloverCycle();
			}
		} else {
			Debug.Log ("Error: Attempt to resolve engagement outside of engagement mode");
		}
	}
	void ExecuteAction(ScriptCharacterSheet hotSheet){
		
		Result result = null;
		
			//Get 1st character in queue and its target
		//ScriptCharacterSheet hotSheet = actingCharacter.GetComponent<ScriptCharacterSheet>();
		if(hotSheet.target){
		
			//Debug.Log (hotSheet.characterID.ToString());
			
			hotSheet.target.GetComponent<ScriptCharacterSheet>().lastAttacker = hotSheet.gameObject;
			
			//Execute appropriate action function
			//if(hotSheet.engageAtRange){
				ScriptCharacterSheet targetSheet = hotSheet.target.GetComponent<ScriptCharacterSheet>();
	
		//Start weapon effect
				hotSheet.gameObject.GetComponentInChildren<ScriptModelController>().SendMessage("WeaponEffect");
	
				//Get action result
				result = GetActionResult(hotSheet, targetSheet);
				
				/*
					scriptInterface.SendMessage("AddNewLine",hotSheet.fullName
			+ " attacks " + targetSheet.fullName + "! " + 
				hotSheet.finalAttack.ToString() + " Accuracy vs. " + targetSheet.evasion.ToString() + " Evasion. " + (result.hitPercentage).ToString() + " % chance.");
				*
				scriptInterface.SendMessage("AddNewLine", result.roll.ToString() + " <> " + result.successNumber.ToString());
				*/
				if(result.success)
				{
					//Reduce health
					targetSheet.meat -= result.damageAmount;

					/*scriptInterface.SendMessage("AddNewLine",hotSheet.fullName
				+ " deals " + result.damageAmount.ToString() + " damage to "+ targetSheet.fullName
				+ ".");*/
				}
				else
				{
					/*
					//Log miss
					scriptInterface.SendMessage("AddNewLine",hotSheet.fullName + " misses!");
					*/
				}
				
			/*	
				//Compare attacker's Attack to target's Defense
			if(hotSheet.accuracy > targetSheet.evasion)
				{
					targetSheet.health -= hotSheet.damage;

			//Log damage
				scriptInterface.SendMessage("AddNewLine",hotSheet.fullName
				+ " deals " + hotSheet.damage.ToString() + " damage to "+ targetSheet.fullName
				+ ". " + targetSheet.health.ToString() + " Health remaining.");
			
				//Launch energy ball
			Transform projectileOrigin = hotSheet.gameObject.transform.FindChild("TraEmitter").transform;
			GameObject hotBall = Instantiate(energyBall, projectileOrigin.position, projectileOrigin.rotation) as GameObject;
			Rigidbody ballRigid = hotBall.GetComponent<Rigidbody>();
			hotBall.GetComponent<Rigidbody>().AddForce(new Vector3(-2500,0,0));
			
			} else {
				scriptInterface.SendMessage("AddNewLine",hotSheet.fullName + " misses!");
				//targetReassess Tactic
				//if(hotSheet.targetReassess){...}	
			}
	*/
					
			
			//	ExecuteRangedAttack(hotSheet);	
			//} else if (hotSheet.engageInMelee){
			//	Debug.Log ("Melee feature pending");
				//ExecuteMeleeAttack(hotSheet);
			//} else {
			//scriptInterface.SendMessage("AddNewLine",hotSheet.fullName + " does zero things.");	
			//}
			

		} else {
			//Character attacks nothing
			scriptInterface.SendMessage("AddNewLine",hotSheet.fullName + " attacks... nothing.");
		}
		//Reset Wait Time to Delay
		hotSheet.waitTime = 1; //Magic number
		
		//Log action
		
		//char1 attacks char 2 (5 ATT vs 3 DEF: 60%). Roll: 52 > 40. char1 shoots char2 for 15 kinetic damage.
		string hotLine = result.actingCharacter.fullName +
			" attacks " +
			result.targetCharacter.fullName +
			" (" +
			result.actingAttack.ToString() +
			" ATT vs " +
			result.targetDefense.ToString() +
			" DEF: " +
			result.hitPercentage.ToString() +
			"%). Roll: " +
			result.roll.ToString();
			
			if(result.success)
		{
			hotLine += " >= " +
			result.successNumber.ToString() + 
			". " +
			result.actingCharacter.fullName + 
			" shoots " +
			result.targetCharacter.fullName + 
			" for " + 
			result.damageAmount.ToString() +
			result.damageType +
			" damage.";	
		}
		else
		{
			hotLine += " < " +
			result.successNumber.ToString() + 
			". " +
			result.actingCharacter.fullName +
					" misses.";
		}
		ConsoleAddLine(hotLine);
		
		//Display damage
			GameObject currentDamageDisplay = Instantiate(damageDisplay, new Vector3(result.targetCharacter.gameObject.transform.position.x,
					result.targetCharacter.gameObject.transform.position.y,damageDisplayDepth), Quaternion.identity) as GameObject;
				 TextMesh statusChangeText = currentDamageDisplay.GetComponentInChildren<TextMesh>();
				
				if(result.success)
				{
					statusChangeText.text = "-" + result.damageAmount + "HP";
				}
				else
				{
					statusChangeText.text = "Miss";
				}
		
	}
	
	//Change cycle
	void RolloverCycle(){
		
				//Begin new Cycle
		cycle += 1;
		//Begin Command Phase
		SetToCommandMode();
		//Stop all movement
		//StartCoroutine("RedLight");
		
		//Reduce all characters' wait time by 1
		foreach(ScriptCharacterSheet hotSheet in charactersInPlay){
			//ScriptCharacterSheet hotSheet = character.GetComponent<ScriptCharacterSheet>();
			if(hotSheet.waitTime > 0)
			{
			hotSheet.waitTime -= 1;
			}
		}

		//Begin cycle timer at zero
		cycleTimer = 0.0F;
		//Log new Cycle
		scriptCycleDisplay.SendMessage("UpdateCycle",cycle);
		//scriptInterface.SendMessage("AddNewLine", "Cycle " + cycle.ToString());
		
		
		
		
		//Ensure all characters have valid targets
		UpdateCharacterValues();
		
		
	}

	
	//Prepare queue
	void GetActiveCharacters(){
		
		activeCharacters = new List<ScriptCharacterSheet>();
		foreach(ScriptCharacterSheet currentSheet in charactersInPlay){
			//ScriptCharacterSheet currentSheet = character.GetComponent<ScriptCharacterSheet>();
			if(currentSheet.waitTime == 0 && currentSheet.isInActingPosition){
				activeCharacters.Add (currentSheet);
			}
		}
		
	}
	void SortActiveCharacters(){
		int initialCount = activeCharacters.Count;
		List<ScriptCharacterSheet> tempList = new List<ScriptCharacterSheet>();
		while(tempList.Count < initialCount){	
			//Determine highest priority of remaining active characters
			float maxPriority = 0.0F;
			for(int i = 0; i < activeCharacters.Count; i++){
			float currentPriority = GetCharacterPriority(activeCharacters[i]);
			if(currentPriority > maxPriority)
				maxPriority = currentPriority;
			}	
		
			//Add highest priority character to temporary list and remove from active characters
			bool findingNextCharacter = true;
			int j = 0;
			while(findingNextCharacter){	
				if(GetCharacterPriority(activeCharacters[j]) == maxPriority){
					tempList.Add (activeCharacters[j]);
					findingNextCharacter = false;
					activeCharacters.RemoveAt (j);
				} else {
					j++;
				}
			}
		}
		
		activeCharacters = tempList;
		
	}
	
	//Maintenence
	void CharacterCleanup(){
		//Debug.Break ();
		List<ScriptCharacterSheet> tempCharactersInPlay = new List<ScriptCharacterSheet>(charactersInPlay);
		//Debug.Log ("temp contains " + tempCharactersInPlay.Count);
		
		for(int i = 0; i < tempCharactersInPlay.Count; i++){
			//Debug.Log (charactersInPlay[1].GetComponent<ScriptCharacterSheet>().characterID + "charactercleanup");
			ScriptCharacterSheet hotSheet = tempCharactersInPlay[i].GetComponent<ScriptCharacterSheet>();
			//Debug.Log(hotSheet.characterID + " has health of " + hotSheet.health.ToString());
			if(hotSheet.meat <= 0){
				KillCharacter(hotSheet);

			}
			
			//Error-checking
			//if(hotSheet.waitTime < 0){
			//Debug.Log ("Error 001: " + hotSheet.characterID.ToString () + "'s waitTime is " + hotSheet.waitTime);	
			//}
		}
	}
	void UpdateCharacterValues(){
		
		//Update targets
		foreach(ScriptCharacterSheet hotSheet in charactersInPlay){
			//ScriptCharacterSheet hotSheet = character.GetComponent<ScriptCharacterSheet>();
			
			if(hotSheet.target == null){
				//For all charactersInPlay without Targets:
				//bool assigningNewTarget = true;
				
				if(charactersInPlay.Count > 1){
				 	
				
				//Choose random character
				
				//int i = (int)Mathf.Floor(Random.value*charactersInPlay.Count);
				//while(assigningNewTarget){
				int randomCharacterIndex = (int)Mathf.Floor(Random.value*charactersInPlay.Count);
				ScriptCharacterSheet otherCharacter = charactersInPlay[randomCharacterIndex];
				
				//If random character is not first character, assign as target (no character can be a target of him/herself
				if(otherCharacter != hotSheet){
						hotSheet.target = otherCharacter;
						//assigningNewTarget = false;
				} else {
					//If it is first character, use next character in line
						hotSheet.target = charactersInPlay[((randomCharacterIndex+1)%(charactersInPlay.Count))];
				}
				}
			}
			
			//Update Equipment modifiers
			hotSheet.netEquipmentAttack = hotSheet.activeItem.netStatProfile.attackModifier;
			hotSheet.netEquipmentDamage = hotSheet.activeItem.netStatProfile.damageModifier;
			//hotSheet.netEquipmentDefense = hotSheet.activeItem.netStatProfile.
			hotSheet.netEquipmentPriority = hotSheet.activeItem.netStatProfile.priorityModifier;
			hotSheet.netEquipmentRange = hotSheet.activeItem.netStatProfile.maxRangeAspect;
	
			//Update Tactic modifiers --only works for first tactic at the moment
			hotSheet.netTacticsAttack = hotSheet.activeTactics[0].modifierProfile.attack;
			hotSheet.netTacticsDamage = hotSheet.activeTactics[0].modifierProfile.damage;
			hotSheet.netTacticsDefense = hotSheet.activeTactics[0].modifierProfile.defense;
			hotSheet.netTacticsPriority = hotSheet.activeTactics[0].modifierProfile.priority;
			hotSheet.netTacticsRange = hotSheet.activeTactics[0].modifierProfile.maxRange;
			
			//Update character's readyStatProfile
			
			hotSheet.readyAttack = hotSheet.baseAttack + hotSheet.netEquipmentAttack + hotSheet.netTacticsAttack;		
			hotSheet.readyDefense = hotSheet.baseDefense + hotSheet.netTacticsDefense;
			hotSheet.readyPriority = hotSheet.nerve + hotSheet.netEquipmentPriority + hotSheet.netTacticsPriority;
			if(hotSheet.activeItem == null)
			{
					hotSheet.readyDamage = hotSheet.unarmedDamage;
					hotSheet.readyRange = hotSheet.unarmedRange;
			}
			else
			{
				hotSheet.readyDamage = hotSheet.netEquipmentDamage + hotSheet.netTacticsDamage;
				hotSheet.readyRange = hotSheet.netEquipmentRange + hotSheet.netTacticsRange;
			}
	
		//Update currentHitChance
			
		}
		
		//Update isInActingPosition
		foreach(ScriptCharacterSheet hotSheet in charactersInPlay)
		{
			//ScriptCharacterSheet hotSheet = character.GetComponent<ScriptCharacterSheet>();
			hotSheet.isInActingPosition = hotSheet.GetComponentInChildren<ScriptControllerTargeting>().GetActingPosition(hotSheet);
		}
		
		
		
						
	}
	
	//void ExecuteRangedAttack(ScriptCharacterSheet hotSheet){
		
	/*Needs revision
	void ExecuteMeleeAttack(ScriptCharacterSheet hotSheet){
		ScriptCharacterSheet targetSheet = hotSheet.target.GetComponent<ScriptCharacterSheet>();
		
						scriptInterface.SendMessage("AddNewLine",hotSheet.fullName
			+ " attacks " + targetSheet.fullName + "! " + 
				hotSheet.melee.ToString() + " Melee vs. " + targetSheet.evasion.ToString() + " Evasion");
			//Compare attacker's Melee to target's Defense
			if(hotSheet.melee > targetSheet.evasion){
				targetSheet.health -= hotSheet.damage;
				scriptInterface.SendMessage("AddNewLine",hotSheet.fullName
				+ " deals " + hotSheet.damage.ToString() + " damage to "+ targetSheet.fullName
				+ ". " + targetSheet.health.ToString() + " Health remaining.");
			} else {
				scriptInterface.SendMessage("AddNewLine",hotSheet.fullName + " misses!");
		}
	}
	*/
	//HANDLER FUNCTIONS
	void ButtonHandler(){
			if(charactersInPlay.Count >= 1){
			ScriptCharacterSheet selectedSheet = charactersInPlay[0].GetComponent<ScriptCharacterSheet>();
					
			string hotButton = inputButtonName;
			//playerPrompt = false;
			inputButtonName = null;
			switch(hotButton){	
				case "Melee":
					//selectedSheet.engageInMelee = true;
			//selectedSheet.engageAtRange = false;
					break;
				case "Ranged":
					//selectedSheet.engageInMelee = false;
			//selectedSheet.engageAtRange = true;
					break;
				case "Next":
					//NextStep();
					SetToExecutionPhase();
					break;
			case null:
				break;
				default:
					Debug.Log ("Error 002: Button name " + hotButton + " is invalid.");
					break;
			}	
		}
		}
		
	//HELPER FUNCTIONS
	
	void ConsoleAddLine(string line)
	{
	scriptInterface.SendMessage("AddNewLine", line);	
	}
	
	bool GetRandomBool(){
		if(Random.value >= .5){
		return true;	
		} else {
		return false;
		}
			
	}
	
	int GetRandom1ToN(int n){
		return (int)Mathf.Floor(Random.value*n + 1);
	}
	
	//int GetRandom1To10(){
	//	return (int)Mathf.Floor(Random.value*10 + 1);
	//}
	
	//int GetRandom1to255(){
	//	int hotValue = (int)Mathf.Floor (Random.value * 255 + 1);
		//Debug.Log (hotValue.ToString());
	//		return hotValue;
	//}
	
	Color GetRandomColor(){
		//Color test = new Color(
	    return new Color(
			Random.value,
			Random.value,
			Random.value,
			255);
			
	}
	
	int GetCharacterPriority(ScriptCharacterSheet hotSheet){
		return hotSheet.nerve;
	}
	
	
	//MODE TOGGLE
	
	void SetToExecutionPhase(){
	//Debug.Log ("Execution Phase");
		
	gamePhase = Phase.Execution;
		
		//Adjust Stats according to Tactics
		//ApplyTactics();
		
		
		//Spawn characters if necessary
		if(spawn00Time == cycle){
		RegisterCharacter(RandomizeCharacterValues(NewCharacter(spawn00)))	;
			//Debug.Log ("RespawnLeft");
		} 
		if(spawn01Time == cycle){
		RegisterCharacter(RandomizeCharacterValues(NewCharacter(spawn01)))	;
			//Debug.Log ("RespawnRight");
		}
		
		//SetToEngagementMode();
		SetToMovementMode();
	}
	
	
	
	void SetToCommandMode(){
		//Debug.Log ("CommandPhase");
		gamePhase = Phase.Command;
		gameMode = Mode.Command;
		//engagementMode = false;
		//movementMode = false;
		//executionPhase = false;
		//commandPhase = true;
		
		
		//Skip Command Phase
		SetToExecutionPhase();
	}
	
	void SetToMovementMode(){
		//Debug.Log ("MovementMode");
		//commandPhase = false;
		engagementMode = false;
		movementMode = true;
		foreach(ScriptCharacterSheet hotSheet in charactersInPlay){
		ScriptCharacterMove hotScript = hotSheet.GetComponent<ScriptCharacterMove>();
			hotScript.greenLight = true;
			hotScript.startLerp = true;
		}
		
	
	}
	
	void SetToEngagementMode(){
		//Debug.Log ("EngagementMode");
		//playerPrompt = false;
		movementMode = false;
		engagementMode = true;
			StartCoroutine("RedLight");
	}
	
	//Wait for every character to finish their frame of movement, then stop all characters
	IEnumerator RedLight(){
		//Debug.Log ("RedLight");
		yield return 0;
		foreach(ScriptCharacterSheet hotSheet in charactersInPlay){
		ScriptCharacterMove hotScript = hotSheet.GetComponent<ScriptCharacterMove>();
			hotScript.greenLight = false;
		}
		if(engagementMode){
		ResolveEngagement ();
		}
		//}
		
	}
	
	//Queries
	bool MovementIsOver(){
		foreach(ScriptCharacterSheet hotSheet in charactersInPlay){
			if(hotSheet.GetComponent<ScriptCharacterMove>().atDestination == false){
				//Debug.Log (character.GetComponent<ScriptCharacterMove>().fracJourney);
				return false;
		}
		}
		return true;
		
	}
	
	
	void ApplyTactics(){
		foreach(ScriptCharacterSheet hotSheet in charactersInPlay){
		//ScriptCharacterSheet hotSheet = character.GetComponent<ScriptCharacterSheet>();
			
			
			//Apply firing mode
			if(hotSheet.aggressiveFire){
				//hotSheet.priority
			} else if(hotSheet.blindFire){
			
			} else if(hotSheet.aimedFire){
				
			}
		}
	}
	
	Result GetActionResult(ScriptCharacterSheet actingCharacter, ScriptCharacterSheet targetCharacter)
	{
		Result result = new Result(actingCharacter);
		result.targetCharacter = targetCharacter;
		
		result.actingAttack = actingCharacter.readyAttack;
		result.targetDefense = targetCharacter.readyDefense;
		
		//Calculate success number
		result.hitPercentage = GetHitPercentage(result.actingAttack, result.targetDefense);
		result.successNumber = 100 - result.hitPercentage;
		
		//Roll d100	
		result.roll = GetRandom1ToN(100);
			
		//If roll is greater than or equal to the success number, attack succeeds
		if(result.roll >= result.successNumber)
		{
			result.success = true;
			//result.hitLocation = GetHitLocation();
			result.damageAmount = actingCharacter.readyDamage;
			result.damageType = DamageType.Kinetic;
		}
		else
		{
			result.success = false;
		}
		/*
			 *result properties: actingCharacter, targetCharacter, damageType, damageAmount, hitLocation, success, status effects, successNumber, roll
		
			Char1 deals 5 kinetic damage to Char2's leg, Char2 is now bleeding
			Char1 misses char2
			*/
		return result;
	}
	
	int GetHitPercentage(int actingAttack, int targetDefense)
	{
		return (10+actingAttack-targetDefense)*5;	
	}
	
	void GiveCharacterItem (ScriptCharacterSheet character, Item hotItem)
	{
		//If item is unowned
		if(hotItem.owner == null)
		{
			//Add to equipped items and set character as new owner
	character.equippedItems.Add (hotItem);
			character.activeItem = hotItem;
			hotItem.owner = character;
		}
		else
		{
			Debug.Log ("Item is already owned");
	}
	
	}
	Item CreateRandomItem()
		{
		Item hotItem = scriptDatabase.GetRandomItem();
		itemsInPlay.Add(hotItem);
		return hotItem;
		}
		
}
