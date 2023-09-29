using HarmonyLib;
using HMLLibrary;
using RaftModLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using WebSocketSharp;
using static SO_TradingPost_Buyable;

namespace TradingPostMod{
public class leathernomore : Mod{
	Harmony har;	
	[ConsoleCommand(name:"trade", docs: "this adds items to trade (name,tier,stock,trashes,coins,amountgiven)")]
	public static void carga(string[] args){
		ItemInstance currentItem = RAPI.GetLocalPlayer().Inventory.GetSelectedHotbarItem();
		if (args.Length==0){
			Debug.LogError("-Current Item in hand : " + currentItem.UniqueName );}
		try{
			string name=args[0];
			if (args.Length==5) name= currentItem.UniqueName;
			int  tier=Int32.Parse(args[args.Length-5]),stock=Int32.Parse(args[args.Length-4]),
				trash=Int32.Parse(args[args.Length-3]),coins=Int32.Parse(args[args.Length-2]),amountgiven=Int32.Parse(args[args.Length-1]);
			Debug.Log(name);
			if(tier<1||tier>3)throw new Exception();
			StreamWriter x= File.AppendText("mods/leatherbuy/leatherpost.ltp");
			x.WriteLine(name+","+tier+","+stock+","+trash+","+coins+","+amountgiven);
			x.Close();
			}

		catch{Debug.Log("please use correct secuence (name, tier, stock, trashes, coins, amountgiven)\n"+
						"in case it's the item on hand use(tier, stock, trash, coins, amount)");}}

	public void Start(){	
		har = new Harmony("com.sirHeimer.leathernomore");
		har.PatchAll();
		Debug.Log("mod leatherbuy added");}



	public void OnModUnload(){har.UnpatchAll("com.sirHeimer.leathernomore");
		Debug.Log("mod leatherbuy removed");}

}
[HarmonyPatch(typeof(TradingPost), nameof(TradingPost.ResetItemsForSale))]
static class buymod{
	static void Prefix(){}
	static void Postfix(TradingPost __instance){
		if (!Directory.Exists("mods/leatherbuy"))Directory.CreateDirectory("mods/leatherbuy");

		if(!File.Exists("mods/leatherbuy/leatherpost.ltp")){
			StreamWriter w= new StreamWriter("mods/leatherbuy/leatherpost.ltp");
			w.WriteLine("Leather,3,10,15,10,1");w.Close();}

		StreamReader rd =new StreamReader("mods/leatherbuy/leatherpost.ltp");
		List<string> lineas = new List<string>();

		for (string linea =rd.ReadLine();linea !=null;linea =rd.ReadLine()) lineas.Add(linea);

		rd.Close();File.WriteAllText("mods/leatherbuy/leatherpost.ltp", string.Empty);

		StreamWriter x= File.AppendText("mods/leatherbuy/leatherpost.ltp");

		try{foreach(string linea in lineas){
			string[] args=linea.Split(',');
			string name=args[0];
			int  tier=Int32.Parse(args[1]),stock=Int32.Parse(args[2]),
				trash=Int32.Parse(args[3]),coins=Int32.Parse(args[4]),amountgiven=Int32.Parse(args[5]);

			SO_TradingPost_Buyable	articulo= ScriptableObject.CreateInstance<SO_TradingPost_Buyable>();
			Item_Base coin = ItemManager.GetItemByName("TradeToken");
			Item_Base cube = ItemManager.GetItemByName("Trashcube");

			switch (tier) {
				case 1:articulo.tier = TradingPost.Tier.Tier1;break;
				case 2:articulo.tier = TradingPost.Tier.Tier2;break;
				case 3:articulo.tier = TradingPost.Tier.Tier3;break;}
		    articulo.startStock = stock;
		   articulo.cost = new[] { new Cost(cube, trash), new Cost(coin, coins) };

			articulo.reward = new Cost(ItemManager.GetItemByName(name), amountgiven);
			Debug.Log(articulo.reward.item.name+" was added to trading post");
			__instance.buyableItems.Add(articulo.CreateInstance());
			x.WriteLine(name+","+tier+","+stock+","+trash+","+coins+","+amountgiven);}}
		catch{}
		x.Close();
}}}