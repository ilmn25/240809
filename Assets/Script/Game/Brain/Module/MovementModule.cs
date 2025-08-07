using Unity.VisualScripting;
using UnityEngine;

public abstract class MovementModule : Module
{
    private StatusModule _statusModule; 
    protected StatusModule StatusModule => _statusModule ??= Machine.GetModule<StatusModule>();
}