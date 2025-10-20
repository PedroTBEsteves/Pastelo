using PrimeTween;
using UnityEngine;

public interface ICustomerPopUpDialogue
{
    Sequence CustomerGaveUpDialogue(Customer customer);
    Sequence CustomerOrderExpiredDialogue(Customer customer);
}
