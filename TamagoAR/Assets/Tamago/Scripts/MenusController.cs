using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenusController : MonoBehaviour
{
    public GameObject CanvasSearching;
    public GameObject CanvasBalance;
    public GameObject CanvasBotMenu; // visibility is connected with searching canvas
    public GameObject CanvasBottomExtras;
    public GameObject CanvasPlacingObject;
    public GameObject AcceptButton;
    public GameObject CancelButton;
    public float flashTimeSeconds = 5f;
    public TextMeshProUGUI StarBalanceText;
    public bool fullMenuOn = false;

    private IEnumerator FlashingBalanceCoroutine;

    public void ShowPlacingMenu()
    {
        CanvasPlacingObject.SetActive(true);
        AcceptButton.SetActive(true);
    }

    public void HidePlacingMenu()
    {
        CanvasPlacingObject.SetActive(false);
        CancelButton.SetActive(false);
        AcceptButton.SetActive(false);
    }

    public void EnableAcceptButton()
    {
        CancelButton.SetActive(false);
        AcceptButton.SetActive(true);
    }

    public void DisableAcceptButton()
    {
        AcceptButton.SetActive(false);
        CancelButton.SetActive(true);
    }

    public void ShowFullMenu()
    {
        CancelBalanceMenuCoroutine();
        CanvasBalance.SetActive(true);
        CanvasBottomExtras.SetActive(true);
    }

    public void ToggleFullMenu()
    {
        if (fullMenuOn)
        {
            HideFullMenu();
        }
        else
        {
            ShowFullMenu();
        }

        fullMenuOn = !fullMenuOn;
    }

    public void HideFullMenu()
    {
        CancelBalanceMenuCoroutine();
        CanvasBottomExtras.SetActive(false);
        CanvasBalance.SetActive(false);
    }

    public void FlashBalanceMenu()
    {
        FlashingBalanceCoroutine = FlashBalanceMenuCoroutine();
        StartCoroutine(FlashingBalanceCoroutine);
    }

    private IEnumerator FlashBalanceMenuCoroutine()
    {
        CanvasBalance.SetActive(true);
        yield return new WaitForSeconds(flashTimeSeconds);
        CanvasBalance.SetActive(false);
    }

    public void ShowSearchingPlanesUI(bool isSearching)
    {
        if (isSearching)
        {
            CanvasBottomExtras.SetActive(false);
            CanvasBotMenu.SetActive(false);
            CanvasSearching.SetActive(true);
            CanvasBalance.SetActive(false);
            CanvasPlacingObject.SetActive(false);
        }
        else
        {
            CanvasSearching.SetActive(false);
            CanvasBotMenu.SetActive(true);
        }
    }

    public void ShowBottomMenu()
    {
        CanvasBotMenu.SetActive(true);
    }

    public void HideBottomMenu()
    {
        CanvasBotMenu.SetActive(false);
    }

    public void UpdateBalanceStarsUI(string balance)
    {
        StarBalanceText.SetText(balance);
    }

    private void CancelBalanceMenuCoroutine()
    {
        if (FlashingBalanceCoroutine != null)
        {
            StopCoroutine(FlashingBalanceCoroutine);
            FlashingBalanceCoroutine = null;
        }
    }

    private void ShowCanvasSearching(bool isVisible)
    {
        CanvasSearching.SetActive(isVisible);
    }

    private void ShowCanvasBalance(bool isVisible)
    {
        CanvasBalance.SetActive(isVisible);
    }

    private void ShowBottomExtrasCanvas(bool isVisible)
    {
        CanvasBottomExtras.SetActive(isVisible);
    }
}