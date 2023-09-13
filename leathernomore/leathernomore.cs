using HarmonyLib;
using HMLLibrary;
using RaftModLoader;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TradingPostMod
{
	public class leathernomore : Mod{
		//mod originally created to add leather to trading post,
	static Traverse<List<SO_TradingPost_Buyable>> t;
	static List<SO_TradingPost_Buyable> lista;
		//first generates a trverse to starting stoc, and a list for the buyable items that we add, as Lista.

	public void Start(){
		//Initializes lista and the traverse, calls for the first time the "resetitmesforsale function"
		//this way the starting stock list has the first items. If it is not called, the original items don't add to list.
		lista=new List<SO_TradingPost_Buyable>();
		//next try catch will throw the mod to not load in case the function 'resetitemsforsale' throws an exception, stops the mod
		//from load to avoid issues on gameplay.
		try{new TradingPost().ResetItemsForSale();}catch{Debug.Log("there was an issue trying to add leather mod");return;}
		t = Traverse.Create<TradingPost>().Field<List<SO_TradingPost_Buyable>>("startingStock");
		leerarchivo(); //we wake up the mod calling the file that contains the items we already have registered.
		Log("leather buy mod has ben loaded");}

	public void OnModUnload(){
		//when unload, each item that we added before, are removed from the traverse starting stock.
		foreach(SO_TradingPost_Buyable i in lista)remover(i);
		Log("buy leather mod has ben unloaded");}

	[ConsoleCommand(name: "trade", docs: "this adds items to trade (name,tier,stock,trashes,coins,amountgiven)")]
	public static void comandoes(string[] args){
		//this function is also called when it's needed to add items to the lista, and the starting stock
		//if we have no idea how the item is called, we can just call "trade" and the log will reply with
			//the name of the item in hand and the sintax to add items.
		if (args.Length==0){
			ItemInstance currentItem = RAPI.GetLocalPlayer().Inventory.GetSelectedHotbarItem();
			Debug.Log("- ][ -Current Item in hand : " + currentItem.settings_Inventory.DisplayName);}

		//this trycatch is in case the comand is not added as requested. Or in case the item was not found.
		try{//the function first converts all the atributes as integers and gives their names.
			string name=args[0];
			int  tier=Int32.Parse(args[1]),stock=Int32.Parse(args[2]),
				trash=Int32.Parse(args[3]),coins=Int32.Parse(args[4]),amountgiven=Int32.Parse(args[5]);

			//in case the tier is not correct, we return the function and display the error message.
			if(tier<1||tier>3){Debug.LogError("please use correct secuence (name, tier, stock, trashes, coins, amountgiven)");return;}
			//this calls the function to add the item to the lists.
			additem(name,tier,stock,trash,coins,amountgiven);

			//later it is added to the file.
			StreamWriter x= File.AppendText("mods/leatherbuy/leatherpost.ltp");
			x.WriteLine(args[0]+","+args[1]+","+args[2]+","+args[3]+","+args[4]+","+args[5]);
			x.Close();
			}catch{Debug.LogError("please use correct secuence (name, tier, stock, trashes, coins, amountgiven)");}}

	public static void additem(string name,int tier,int stock,int trash,int coins,int amountgiven){
		//this is the function that adds the item to lists. initializes a new buyable object.
		SO_TradingPost_Buyable	articulo= ScriptableObject.CreateInstance<SO_TradingPost_Buyable>();
		//items base cube and coins.
		Item_Base coin = ItemManager.GetItemByName("TradeToken");
        Item_Base cube = ItemManager.GetItemByName("Trashcube");
		//this is where we use cubes and coins, add costs. trash is the amount, same as coins with S. later we register the stock
        articulo.cost = new[] { new Cost(cube, trash), new Cost(coin, coins) };
        articulo.startStock = stock;

		//we select the tier of the item. tier is checked before. so it's not needed a default handler.
        switch (tier) {
			case 1:articulo.tier = TradingPost.Tier.Tier1;break;
			case 2:articulo.tier = TradingPost.Tier.Tier2;break;
			case 3:articulo.tier = TradingPost.Tier.Tier3;break;}

		//we register a reward, with the name of the item, and the amount.
		articulo.reward = new Cost(ItemManager.GetItemByName(name), amountgiven);
		//next line will throw an exception in case item is not found in item manager
		//and trycatch from comands function will return the error message.
		Debug.Log(articulo.reward.item.name+" was added to trading post");
		//then we add the item to both lists, the traverse that contains every item in trading post.
		//and the lista that contains the items adde by this mod.
		lista.Add(articulo);
		t.Value.Add(articulo);}

	public void remover(SO_TradingPost_Buyable articulo){
		//this is the function to remove items at the startingstock
		t.Value.RemoveAt(t.Value.IndexOf(articulo));}

	public void leerarchivo(){
		//check if file exists, creates if doesn't exist, same with directory.
		if (!Directory.Exists("mods/leatherbuy"))Directory.CreateDirectory("mods/leatherbuy");
		if(!File.Exists("mods/leatherbuy/leatherpost.ltp")){
			StreamWriter x= new StreamWriter("mods/leatherbuy/leatherpost.ltp");
			x.WriteLine("Leather,3,10,15,10,1");x.Close();}

		//migrates every line of the file to a list.
		StreamReader rd =new StreamReader("mods/leatherbuy/leatherpost.ltp");
		List<string> lineas = new List<string>();
		for (string linea =rd.ReadLine();linea !=null;linea =rd.ReadLine()) lineas.Add(linea);
		rd.Close();File.WriteAllText("mods/leatherbuy/leatherpost.ltp", string.Empty);

		//calls the function to add items to trading post, and create
		foreach(string linea in lineas)comandoes(linea.Split(','));}}}