using HarmonyLib;
using HMLLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace TradingPostMod
{
	public class leathernomore : Mod{
    Harmony harmony;
	int indx;
	bool stat;
    SO_TradingPost_Buyable st;
	[HarmonyPatch(typeof(TradingPost),nameof(TradingPost.ResetItemsForSale ))]
    public void Awake(){
		stat=true;
		if(stat){(harmony = new Harmony("com.aidanamite.leathernomore")).PatchAll();
        var t = Traverse.Create<TradingPost>().Field<List<SO_TradingPost_Buyable>>("startingStock");
		new TradingPost().ResetItemsForSale();
        if (t.Value.Count !=0){
			indx=t.Value.Count;
			t.Value = Patch_ResetTradingPostItems.ModifyBuyableItems(t.Value);
			st=t.Value[indx];}
        Log("Mod has been loaded!");
		stat=false;}}
    public void OnModUnload(){
        if(!stat)harmony.UnpatchAll(harmony.Id);
		var t = Traverse.Create<TradingPost>().Field<List<SO_TradingPost_Buyable>>("startingStock");
        if (t.Value != null){t.Value.RemoveAt(t.Value.IndexOf(st));}
        Log("Mod has been unloaded!");}}
	static class Patch_ResetTradingPostItems{
        static List<SO_TradingPost_Buyable> _b;
        public static List<SO_TradingPost_Buyable> CustomBuyables{
            get{
                if (_b == null){
                    _b = new List<SO_TradingPost_Buyable>();
                    Item_Base coin = ItemManager.GetItemByName("TradeToken");
                    Item_Base cube = ItemManager.GetItemByName("Trashcube");

                    var i = ScriptableObject.CreateInstance<SO_TradingPost_Buyable>();
                    i.cost = new[] { new Cost(cube, 15), new Cost(coin, 5) };
                    i.startStock = 10;
                    i.tier = TradingPost.Tier.Tier3;
                    i.reward = new Cost(ItemManager.GetItemByName("Leather"), 1);
                    _b.Add(i);}
                return _b;}}
    
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions){
            var code = instructions.ToList();
            var ind = code.FindIndex(x => x.opcode == OpCodes.Stsfld && x.operand is FieldInfo f && f.Name == "startingStock");
            code.Insert(ind, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_ResetTradingPostItems), nameof(ModifyBuyableItems))));
            return code;}
    
        public static List<SO_TradingPost_Buyable> ModifyBuyableItems(List<SO_TradingPost_Buyable> original){
            foreach (var i in CustomBuyables)
                if (i.reward.item)
                    original.Add(i);
            return original;}}}