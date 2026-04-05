using KBCore.Refs;
using UnityEngine;

public class StrikeIcon : ValidatedMonoBehaviour
{
    [SerializeField]
    private GameObject _target;

    public void SetTaken(bool taken)
    {
        if (_target == null)
        {
            Debug.LogError($"{nameof(StrikeIcon)} on '{name}' is missing its target.", this);
            return;
        }

        _target.SetActive(!taken);
    }
}
