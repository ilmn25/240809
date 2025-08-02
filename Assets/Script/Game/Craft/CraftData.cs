using System.Collections.Generic;

public partial class Craft
{
    public int Stack;
    public Dictionary<string, int> Ingredients;
    public string[] Modifiers;

    public Craft(Dictionary<string, int> ingredients, int stack, string[] modifiers)
    {
        Stack = stack;
        Ingredients = ingredients;
        Modifiers = modifiers;
    }
}