using UnityEngine;
using UnityEngine.UI;

public class SwitchPage : MonoBehaviour
{
    public GameObject[] cards;
    [SerializeField] GameObject _leftButton;
    [SerializeField] GameObject _rightButton;
    GameObject _cardNow;
    int index;
    void OnEnable()
    {
        index = 0;
        _cardNow = cards[index];
        _cardNow.SetActive(true);
        _rightButton.SetActive(true);
        Debug.Log("index now: " + index);
    }
    void OnDisable() {
        _cardNow.SetActive(false);
        _leftButton.SetActive(false);
        _rightButton.SetActive(false);
    }
    public void SwipeRight()
    {
        if(_cardNow.activeSelf)
            _cardNow.SetActive(false);
        if(!_leftButton.activeSelf)
            _leftButton.SetActive(true);

        index++;
        if(index >= cards.Length - 1)
            _rightButton.SetActive(false);

        _cardNow = cards[index];
        _cardNow.SetActive(true);
        Debug.Log("index now: " + index);
    }
    public void SwipeLeft()
    {
        if (_cardNow.activeSelf)
            _cardNow.SetActive(false);
        if (!_rightButton.activeSelf)
            _rightButton.SetActive(true);

        index--;
        if (index <= 0)
            _leftButton.SetActive(false);


        _cardNow = cards[index];
        _cardNow.SetActive(true);
        Debug.Log("index now: " + index);
    }
}
