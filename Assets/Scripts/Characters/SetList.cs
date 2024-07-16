using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// The SetList class controls the game store
/// </summary>
public class SetList : MonoBehaviour
{

    #region Fields
    private int _currentSet = -1;
    private int _characterSet = -1;
    public int characterSet { get => _characterSet; set { _characterSet = value; } }

    [SerializeField]
    private List<CharacterSet> setList;

    #endregion

    #region MonoBehaviour Methods

    private void Start()
    {
        HUDManager.instance.setChangeClick = ChangeSet;
        HUDManager.instance.closeMenuClick = BuySet;
    }

    #endregion

    #region Public Methods
    /// <summary>
    /// Changes the character set
    /// </summary>
    /// <param name="indicator">-1 for previous set, +1 to next set</param>
    public void ChangeSet(int indicator)
    {

        //Remove old set
        if (_characterSet >=0)
        {
            setList[_characterSet].face.SetActive(false);
            setList[_characterSet].head.SetActive(false);
            setList[_characterSet].top.SetActive(false);
            setList[_characterSet].bottom.SetActive(false);
        }

        HUDManager.instance.TMP_AcceptCaptionButton.text = "Close";

        if (characterSet + indicator >= setList.Count)
        {
            characterSet = -1;
            return;
        }


        if (characterSet + indicator < -1)
        {
            characterSet = setList.Count;
        }

        characterSet += indicator;

        //Vest a new set
        if (_characterSet >= 0)
        {
            setList[_characterSet].face.SetActive(true);
            setList[_characterSet].head.SetActive(true);
            setList[_characterSet].top.SetActive(true);
            setList[_characterSet].bottom.SetActive(true);
            HUDManager.instance.TMP_AcceptCaptionButton.text = "$" + setList[_characterSet].price;
        }


    }

    public void BuySet(bool status)
    {
        //Verify if have changes
        if(_currentSet != _characterSet)
        {
            if (status)//Try buy items
            {
                //Check player balance
                if (LevelManager.coins >= setList[_characterSet].price)
                {
                    LevelManager.coins -= setList[_characterSet].price;  //subtract amount from balance
                    _currentSet = _characterSet; //Sets the new set
                }
                else //Not enough money
                {
                    HUDManager.instance.ShowMessage(HUDManager.instance.TMP_MenuMessage, "Not enought money!", 3f);
                    return;
                }

            }
            else //Player canceled the transaction, return to original set.
            {
                setList[_characterSet].face.SetActive(false);
                setList[_characterSet].head.SetActive(false);
                setList[_characterSet].top.SetActive(false);
                setList[_characterSet].bottom.SetActive(false);

                if(_currentSet >= 0)
                {
                    setList[_currentSet].face.SetActive(true);
                    setList[_currentSet].head.SetActive(true);
                    setList[_currentSet].top.SetActive(true);
                    setList[_currentSet].bottom.SetActive(true);
                }
            }
        }

        HUDManager.instance.menuPanel.SetActive(false);
    }

    #endregion
}
