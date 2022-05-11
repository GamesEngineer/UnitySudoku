using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SudokuCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GridLayoutGroup markupGrid;
    public TextMeshProUGUI numberText;
    public SudokuGrid sudokuGrid;
    private Image myImage;

    public int Slot => transform.GetSiblingIndex();
    public int Row => Slot / 9;
    public int Col => Slot % 9;
    public float Age { get; private set; }
    public int Number
    {
        get => _number;
        set {
            if (_number == value) return;
            _number = value;
            Age = 0f;
            sudokuGrid.SetGameValue(Row, Col, _number);
            numberText.text = _number.ToString();
        }
    }
    private int _number;

    public static SudokuCell SelectedCell { get; private set; }

    private void Awake()
    {
        myImage = GetComponent<Image>();
        if (!sudokuGrid)
        {
            sudokuGrid = FindObjectOfType<SudokuGrid>();
        }
        sudokuGrid.OnPuzzleCreated += SudokuGrid_OnPuzzleCreatedOrSolved;
        sudokuGrid.OnPuzzleSolved += SudokuGrid_OnPuzzleCreatedOrSolved;
    }

    private void OnDestroy()
    {
        sudokuGrid.OnPuzzleCreated -= SudokuGrid_OnPuzzleCreatedOrSolved;
        sudokuGrid.OnPuzzleSolved -= SudokuGrid_OnPuzzleCreatedOrSolved;
    }

    private void SudokuGrid_OnPuzzleCreatedOrSolved()
    {
        Number = sudokuGrid.GetGameValue(Row, Col);
        ClearMarkup();
    }

    private void Start()
    {
        ClearMarkup();
    }

    private void Update()
    {
        Age = sudokuGrid.showHints ? Age + Time.deltaTime : 0f;
        ShowMarkup(Number == 0);
        UpdateCellColor();
    }

    private void UpdateCellColor()
    {
        myImage.color = (SelectedCell == this) ? Color.yellow : new Color(0f, 0f, 0f, 0.25f);

        if (Number == 0) return;

        int puzzleValue = sudokuGrid.GetPuzzleValue(Row, Col);
        if (puzzleValue == 0)
        {
            if (!sudokuGrid.IsValid(Row, Col))
            {
                numberText.color = Color.red;
            }
            else
            {
                int solutionValue = sudokuGrid.GetSolutionValue(Row, Col);
                if (Age < sudokuGrid.ageBeforeHints || solutionValue <= 0 || solutionValue == Number)
                {
                    numberText.color = Color.blue;
                }
                else
                {
                    numberText.color = Color.yellow;
                }
            }
        }
        else if (sudokuGrid.IsValid(Row, Col))
        {
            numberText.color = Color.black;
        }
        else
        {
            numberText.color = new Color(0.7f, 0f, 0f, 1f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SelectedCell = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (SelectedCell == this)
        {
            SelectedCell = null;
        }
    }

    public void ShowMarkup(bool yesNo)
    {
        markupGrid.gameObject.SetActive(yesNo);
        numberText.gameObject.SetActive(!yesNo);
    }

    public void ClearMarkup()
    {
        for (int n = 1; n <= 9; n++)
        {
            GameObject markup = GetMarkupButtonLabel(n);
            markup.SetActive(false);
        }
    }

    public void ToggleMarkupDigit(int digit)
    {
        GameObject buttonLabel = GetMarkupButtonLabel(digit);
        buttonLabel.SetActive(!buttonLabel.activeSelf);
    }

    private GameObject GetMarkupButtonLabel(int digit)
    {
        int index = digit - 1;
        var child = markupGrid.transform.GetChild(index);
        var button = child.GetComponent<Button>();
        var buttonLabel = button.transform.GetChild(0).gameObject;
        return buttonLabel;
    }   
}
