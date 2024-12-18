using System.Collections.Generic;

public class CraftData
{
    public int Stack;
    public Dictionary<string, int> Ingredients;
    public string[] Modifiers;

    public CraftData(Dictionary<string, int> ingredients, int stack, string[] modifiers)
    {
        Stack = stack;
        Ingredients = ingredients;
        Modifiers = modifiers;
    }
}