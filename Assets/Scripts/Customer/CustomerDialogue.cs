using System;
using System.Collections.Generic;
using System.Text;
using PrimeTween;
using Reflex.Attributes;
using TMPro;
using UnityEngine;

public class CustomerDialogue : MonoBehaviour, ICustomerDialogue
{
    [SerializeField]
    private SpriteRenderer _customerSprite;

    [SerializeField]
    private GameObject _dialogueObject;
    
    [SerializeField]
    private TextMeshProUGUI _text;

    [SerializeField]
    private TextAsset[] _orderDialogueTemplates;

    [SerializeField]
    private TextAsset[] _emptyPastelOrder;
    
    [SerializeField]
    private TextAsset[] _correctOrderDialogues;
    
    [SerializeField]
    private TextAsset[] _incorrectOrderDialogues;

    [SerializeField]
    private GameObject _deliveryBag;
    
    [SerializeField]
    private AudioSource _audioSource;

    [SerializeField]
    private float _delayAfterTextIsDone = 1f;

    [Inject]
    private DialogueWriter _dialogueWriter;
    
    [Inject]
    private readonly CustomerQueue _customerQueue;
    
    private Sequence _dialogueSequence;

    public bool IsPlaying => _dialogueSequence.isAlive;

    private void Start()
    {
        _customerQueue.CustomerArrived += customer =>
        {
            if (_customerSprite != null)
                _customerSprite.sprite = customer.Sprite;
        };

        _customerQueue.CustomerExpired += customer =>
        {
            if (_customerQueue.TryPeek(out var nextCustomer))
                _customerSprite.sprite = nextCustomer.Sprite;
            else
                _customerSprite.sprite = null;
        };
    }

    public Sequence OrderDialogue(Order order)
    {
        _customerSprite.sprite = order.Customer.Sprite;

        string text;

        if (order.Recipe.Fillings.Count == 0)
        {
            text = _emptyPastelOrder.GetRandomElement().text;
        }
        else
        {
            var template = _orderDialogueTemplates.GetRandomElement();

            text = string.Format(template.text, order.Recipe.Dough.Name, GetFillingsText(order.Recipe.Fillings));
        }
        
        _dialogueObject.SetActive(true);

        _dialogueSequence = _dialogueWriter.WriteText(text, _text, _audioSource)
            .Chain(Tween.Delay(_delayAfterTextIsDone, () =>
            {
                _dialogueObject.SetActive(false);
                if (_customerQueue.TryPeek(out var nextCustomer))
                    _customerSprite.sprite = nextCustomer.Sprite;
                else
                    _customerSprite.sprite = null;
            }));
        
        return _dialogueSequence;
    }

    public Sequence DeliveryDialogue(Order order, Delivery delivery, OrderController orderController)
    {
        _customerSprite.sprite = order.Customer.Sprite;
        _deliveryBag.SetActive(true);

        var dialogues = delivery.IsCorrectFor(order) ? _correctOrderDialogues : _incorrectOrderDialogues;
        var dialogue = dialogues.GetRandomElement();
        
        _dialogueSequence = Sequence.Create(Tween.Delay(2f, () =>
        {
            _dialogueObject.SetActive(true);
            
            orderController.DeliverOrder(order, delivery);
        }))
        .Chain(_dialogueWriter.WriteText(dialogue.text, _text, _audioSource))
        .Chain(Tween.Delay(2f, () =>
        {
            _dialogueObject.SetActive(false);
            if (_customerQueue.TryPeek(out var nextCustomer))
                _customerSprite.sprite = nextCustomer.Sprite;
            else
                _customerSprite.sprite = null;
            _deliveryBag.SetActive(false);
        }));

        return _dialogueSequence;
    }

    private static string GetFillingsText(IReadOnlyDictionary<Filling, int> fillings)
    {
        var builder = new StringBuilder();

        foreach (var (filling, amount) in fillings)
        {
            builder.Append($" {amount} {GetFillingName(filling, amount)},");
        }
        
        builder.Remove(builder.Length - 1, 1);
        
        return builder.ToString();
    }
    
    private static string GetFillingName(Filling filling, int amount) => amount > 1 ? filling.PluralName : filling.Name;
}
