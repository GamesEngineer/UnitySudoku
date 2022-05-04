using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SudokuCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GridLayoutGroup markupGrid;
    public TextMeshProUGUI numberText;
    public static SudokuCell selectedCell;
    public SudokuGrid sudokuGrid;

    public int Number
    {
        get => number;
        set {
            number = value;
            sudokuGrid.SetGameValue(Row, Col, number);
            numberText.text = number.ToString();
        }
    }
    private int number;

    private void Awake()
    {
        if (!sudokuGrid)
        {
            sudokuGrid = FindObjectOfType<SudokuGrid>();
        }
    }

    private void Start()
    {
        for (int n = 0; n < 9; n++)
        {
            var label = GetMarkupButtonLabel(n);
            label.SetActive(false);
        }
        Number = sudokuGrid.GetGameValue(Row, Col);
        ShowMarkup(number == 0);
    }

    private void Update()
    {
        if (selectedCell == this && !sudokuGrid.IsSolved())
        {
            UpdateNumberFromInput();
        }
        ShowMarkup(number == 0);

        if (number != 0)
        {
            int puzzleValue = sudokuGrid.GetPuzzleValue(Row, Col);
            if (puzzleValue == 0)
            {
                numberText.color = sudokuGrid.IsValid(Row, Col) ? Color.blue : Color.red;
            }
            else
            {
                numberText.color = sudokuGrid.IsValid(Row, Col) ? Color.black : new Color(0.7f, 0f, 0f, 1f);
            }
        }
    }

    private void UpdateNumberFromInput()
    {
        int puzzleValue = sudokuGrid.GetPuzzleValue(Row, Col);
        if (puzzleValue != 0) return;

        for (int i = 0; i < Input.inputString.Length; i++)
        {
            char c = Input.inputString[i];
            switch (c)
            {
                case '0': Number = 0; break;
                case '1': Number = 1; break;
                case '2': Number = 2; break;
                case '3': Number = 3; break;
                case '4': Number = 4; break;
                case '5': Number = 5; break;
                case '6': Number = 6; break;
                case '7': Number = 7; break;
                case '8': Number = 8; break;
                case '9': Number = 9; break;
                default: break;
            }
        }
    }

    public int Row => transform.GetSiblingIndex() / 9;
    public int Col => transform.GetSiblingIndex() % 9;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        selectedCell = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (selectedCell == this)
        {
            selectedCell = null;
        }
    }

    public void ShowMarkup(bool yesNo)
    {
        markupGrid.gameObject.SetActive(yesNo);
        numberText.gameObject.SetActive(!yesNo);
    }

    public void ToggleMarkup(int n)
    {
        GameObject buttonLabel = GetMarkupButtonLabel(n);
        buttonLabel.SetActive(!buttonLabel.activeSelf);
    }

    private GameObject GetMarkupButtonLabel(int siblingIndex)
    {
        var child = markupGrid.transform.GetChild(siblingIndex);
        var button = child.GetComponent<Button>();
        var buttonLabel = button.transform.GetChild(0).gameObject;
        return buttonLabel;
    }   
}
