using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class SudokuController : MonoBehaviour
{
    public GameObject passDialog;
    public GameObject playManager;
    public GameObject mainPanel;

    public CellManager cellManager;
    public MemoManager memoManager;
    public SudokuMaker sudokuMaker;

    public int[,] sudoku;
    public int[,] fullSudoku;
    public int[,,] memoSudoku;

    public List<Tuple<int[,], int[,,]>> lateSudoku = new List<Tuple<int[,], int[,,]>>();
    public int undoIndex = -1;

    // 링크/체인용 변수
    public List<Tuple<int, int>> tracer;
    public List<List<Tuple<int, int>>> tracerList;
    public List<Tuple<int, int>> chains;
    protected virtual void Start()
    {
        sudoku = SudokuManager.sudoku;
        fullSudoku = SudokuManager.fullSudoku;
        memoSudoku = SudokuManager.memoSudoku;
    }

    #region 값 유무 확인
    public bool IsInCell(int y, int x, int value)
    {
        return sudoku[y, x] == value;
    }

    public bool IsInMemoCell(int y, int x, int value)
    {
        if (value == 0)
        {
            return false;
        }
        return memoSudoku[value - 1, y, x] == 1;
    }

    #endregion

    #region 두 셀 비교
    public bool IsEqualMemoCell(Tuple<int, int> c1, Tuple<int, int> c2)
    {
        var l1 = GetActiveMemoValue(c1.Item1, c1.Item2);
        var l2 = GetActiveMemoValue(c2.Item1, c2.Item2);

        if (c1.Equals(c2))
        {
            return false;
        }

        if (l1.Count != l2.Count)
        {
            return false;
        }

        for (int pin = 0; pin < l1.Count; pin++)
        {
            if (l1[pin] != l2[pin])
            {
                return false;
            }
        }

        return true;
    }

    public List<Tuple<int, int>> GetAllEqualMemoCell(int y, int x)
    {
        List<Tuple<int, int>> list = new List<Tuple<int, int>>();
        for (int _y = 0; _y < 9; _y++)
        {
            var emc_row = GetEmptyCellsInRow(_y);
            foreach (var emc in emc_row)
            {
                if (IsEqualMemoCell(new Tuple<int, int>(y, x), new Tuple<int, int>(_y, emc)))
                {
                    list.Add(new Tuple<int, int>(_y, emc));
                }
            }
        }
        return list;
    }

    #endregion

    #region 스도쿠 완성 여부 검사
    public bool IsCompleteRow(int y)
    {
        bool[] tuple = new bool[9];

        for (int x = 0; x < 9; x++)
        {
            int v = cellManager.sudoku[y, x];
            if (v != 0)
            {
                tuple[v - 1] = true;
            }
        }

        for (int index = 0; index < 9; index++)
        {
            if (tuple[index] == false)
            {
                return false;
            }
        }

        return true;
    }
    public bool IsCompleteCol(int x)
    {
        bool[] tuple = new bool[9];

        for (int y = 0; y < 9; y++)
        {
            int v = cellManager.sudoku[y, x];
            if (v != 0)
            {
                tuple[v - 1] = true;
            }
        }

        for (int index = 0; index < 9; index++)
        {
            if (tuple[index] == false)
            {
                return false;
            }
        }

        return true;
    }
    public bool IsCompleteSG(int y, int x)
    {
        bool[] tuple = new bool[9];

        for (int _y = y * 3; _y < y * 3 + 3; _y++)
        {
            for (int _x = x * 3; _x < x * 3 + 3; _x++)
            {
                int v = cellManager.sudoku[_y, _x];
                if (v != 0)
                {
                    tuple[v - 1] = true;
                }
            }
        }

        for (int index = 0; index < 9; index++)
        {
            if (tuple[index] == false)
            {
                return false;
            }
        }

        return true;
    }
    public bool IsSudokuComplete()
    {
        bool t;
        for (int y = 0; y < 9; y++)
        {
            t = IsCompleteRow(y);
            if (!t)
            {
                return false;
            }
        }

        for (int x = 0; x < 9; x++)
        {
            t = IsCompleteCol(x);
            if (!t)
            {
                return false;
            }
        }

        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                t = IsCompleteSG(y, x);
                if (!t)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public bool FinishSudoku()
    {
        if (IsSudokuComplete())
        {
            passDialog.SetActive(true);
            playManager.SetActive(false);

            mainPanel.transform.Find("ManualTools").gameObject.SetActive(false);
            mainPanel.transform.Find("NumberHighlighter").gameObject.SetActive(false);
            mainPanel.transform.Find("AutoTools").gameObject.SetActive(false);
            mainPanel.transform.Find("Finisher").Find("MainMenuButton").gameObject.SetActive(true);
            mainPanel.transform.Find("Finisher").Find("SaveButton").gameObject.SetActive(false);
            mainPanel.transform.Find("HelpButton").gameObject.SetActive(false);

            return true;
        }
        return false;
    }
    #endregion

    #region 스도쿠 정상 여부 검사
    public bool IsNormalRow(int y)
    {
        List<int> nums = new List<int>();
        for (int _x = 0; _x < 9; _x++)
        {
            int val = sudoku[y, _x];
            if (val != 0 && nums.Contains(val))
            {
                return false;
            }
            else
            {
                nums.Add(val);
            }
        }
        return true;
    }
    public bool IsNormalCol(int x)
    {
        List<int> nums = new List<int>();
        for (int _y = 0; _y < 9; _y++)
        {
            int val = sudoku[_y, x];
            if (val != 0 && nums.Contains(val))
            {
                return false;
            }
            else
            {
                nums.Add(val);
            }
        }
        return true;
    }
    public bool IsNormalSG(int y, int x)
    {
        List<int> nums = new List<int>();
        for (int _y = y * 3; _y < y * 3 + 3; _y++)
        {
            for (int _x = x * 3; _x < x * 3 + 3; _x++)
            {
                int val = sudoku[_y, _x];
                if (val != 0 && nums.Contains(val))
                {
                    return false;
                }
                else
                {
                    nums.Add(val);
                }
            }
        }
        return true;
    }

    public bool IsNormalSudoku()
    {
        for (int y = 0; y < 9; y++)
        {
            if (!IsNormalRow(y))
            {
                return false;
            }
        }

        for (int x = 0; x < 9; x++)
        {
            if (!IsNormalCol(x))
            {
                return false;
            }
        }
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                if (!IsNormalSG(y, x))
                {
                    return false;
                }
            }
        }

        return true;
    }
    #endregion

    #region 스도쿠 무결성 검사 newval == 1~9
    public bool IsNewValueAvailableRow(int y, int x, int newVal, List<Tuple<int, int>> list = null)
    {
        bool flag = true;
        for (int _x = 0; _x < 9; _x++)
        {
            if (x != _x && cellManager.sudoku[y, _x] == newVal)
            {
                if (list != null)
                {
                    list.Add(new Tuple<int, int>(y, _x));
                }
                flag = false;
            }
        }
        return flag;
    }
    public bool IsNewValueAvailableCol(int y, int x, int newVal, List<Tuple<int, int>> list = null)
    {
        bool flag = true;
        for (int _y = 0; _y < 9; _y++)
        {
            if (y != _y && cellManager.sudoku[_y, x] == newVal)
            {
                if (list != null)
                {
                    list.Add(new Tuple<int, int>(_y, x));
                }
                flag = false;
            }
        }
        return flag;
    }
    public bool IsNewValueAvailableSG(int y, int x, int newVal, List<Tuple<int, int>> list = null)
    {
        bool flag = true;
        int ty = y / 3;
        int tx = x / 3;
        for (int _y = ty * 3; _y < ty * 3 + 3; _y++)
        {
            for (int _x = tx * 3; _x < tx * 3 + 3; _x++)
            {
                if (y != _y && x != _x && cellManager.sudoku[_y, _x] == newVal)
                {
                    if (list != null)
                    {
                        list.Add(new Tuple<int, int>(_y, _x));
                    }
                    flag = false;
                }
            }
        }
        return flag;
    }
    public void CheckNewValueNormal(int y, int x, int newVal)
    {
        List<Tuple<int, int>> list = new List<Tuple<int, int>>();
        IsNewValueAvailableRow(y, x, newVal, list);
        IsNewValueAvailableCol(y, x, newVal, list);
        IsNewValueAvailableSG(y, x, newVal, list);

        for (int index = 0; index < list.Count; index++)
        {
            cellManager.Twinkle(list[index].Item1, list[index].Item2);
        }
    }
    public List<Tuple<int, int>> CompareWithFullSudoku()
    {
        sudokuMaker = new SudokuMaker();

        List<Tuple<int, int>> points = new List<Tuple<int, int>>();
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                if (sudoku[y, x] != 0 &&
                    sudoku[y, x] != fullSudoku[y, x])
                {
                    points.Add(new Tuple<int, int>(y, x));
                }
            }
        }
        return points;
    }
    public List<Tuple<int, int>> CompareMemoWithFullSudoku()
    {
        sudokuMaker = new SudokuMaker();

        List<Tuple<int, int>> points = new List<Tuple<int, int>>();
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                if (sudoku[y, x] == 0) // 스도쿠 값이 없을 때
                {
                    int rightNumber = fullSudoku[y, x];

                    if (memoSudoku[rightNumber - 1, y, x] == 0)
                    {
                        points.Add(new Tuple<int, int>(y, x));
                    }
                }
            }
        }
        return points;
    }
    #endregion

    #region 빈 값 반환
    public List<int> GetEmptyValueInRow(int y)
    {
        int[] n = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        List<int> empty = new List<int>(n);
        for (int _x = 0; _x < 9; _x++)
        {
            if (sudoku[y, _x] != 0)
            {
                empty.Remove(sudoku[y, _x]);
            }
        }
        return empty;
    }
    public List<int> GetEmptyValueInCol(int x)
    {
        int[] n = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        List<int> empty = new List<int>(n);
        for (int _y = 0; _y < 9; _y++)
        {
            if (sudoku[_y, x] != 0)
            {
                empty.Remove(sudoku[_y, x]);
            }
        }
        return empty;
    }
    public List<int> GetEmptyValueInSG(int y, int x) //
    {
        int[] n = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        List<int> empty = new List<int>(n);
        for (int _y = y * 3; _y < y * 3 + 3; _y++)
        {
            for (int _x = x * 3; _x < x * 3 + 3; _x++)
            {
                if (sudoku[_y, _x] != 0)
                {
                    empty.Remove(sudoku[_y, _x]);
                }
            }
        }
        return empty;
    }
    #endregion

    #region 빈 셀 반환
    public bool IsEmptyCell(int y, int x)
    {
        if (sudoku[y, x] == 0)
        {
            return true;
        }
        return false;
    }

    public List<int> GetEmptyCellsInRow(int y)
    {
        List<int> empty = new List<int>();
        for (int _x = 0; _x < 9; _x++)
        {
            if (IsEmptyCell(y, _x))
            {
                empty.Add(_x);
            }
        }
        return empty;
    }
    public List<int> GetEmptyCellsInCol(int x)
    {
        List<int> empty = new List<int>();
        for (int _y = 0; _y < 9; _y++)
        {
            if (IsEmptyCell(_y, x))
            {
                empty.Add(_y);
            }
        }
        return empty;
    }
    public List<Tuple<int, int>> GetEmptyCellsInSG(int y, int x) //
    {
        List<Tuple<int, int>> empty = new List<Tuple<int, int>>();
        for (int _y = y * 3; _y < y * 3 + 3; _y++)
        {
            for (int _x = x * 3; _x < x * 3 + 3; _x++)
            {
                if (IsEmptyCell(_y, _x))
                {
                    empty.Add(new Tuple<int, int>(_y, _x));
                }
            }
        }
        return empty;
    }
    #endregion

    #region 메모 데이터 반환
    public int[] GetMemoRow(int y, int value)
    {
        int[] row = new int[9];
        for (int _x = 0; _x < 9; _x++)
        {
            row[_x] = memoSudoku[value - 1, y, _x];
        }
        return row;
    }
    public int[] GetMemoCol(int x, int value)
    {
        int[] col = new int[9];
        for (int _y = 0; _y < 9; _y++)
        {
            col[_y] = memoSudoku[value - 1, _y, x];
        }
        return col;
    }
    public int[] GetMemoSG(int y, int x, int value)
    {
        int[] SG = new int[9];
        int cnt = 0;
        for (int _y = y * 3; _y < y * 3 + 3; _y++)
        {
            for (int _x = x * 3; _x < x * 3 + 3; _x++)
            {
                SG[cnt++] = memoSudoku[value - 1, _y, _x];
            }
        }
        return SG;
    }
    #endregion

    #region 값이 존재하는 메모 셀 반환
    public List<int> GetMemoCellInRow(int y, int value)
    {
        List<int> list = new List<int>();
        for (int _x = 0; _x < 9; _x++)
        {
            if (IsInMemoCell(y, _x, value))
            {
                list.Add(_x);
            }
        }
        return list;
    }
    public List<int> GetMemoCellInCol(int x, int value)
    {
        List<int> list = new List<int>();
        for (int _y = 0; _y < 9; _y++)
        {
            if (IsInMemoCell(_y, x, value))
            {
                list.Add(_y);
            }
        }
        return list;
    }
    public List<Tuple<int, int>> GetMemoCellInSG(int y, int x, int value)
    {
        List<Tuple<int, int>> list = new List<Tuple<int, int>>();

        for (int _y = y * 3; _y < y * 3 + 3; _y++)
        {
            for (int _x = x * 3; _x < x * 3 + 3; _x++)
            {
                if (IsInMemoCell(_y, _x, value))
                {
                    list.Add(new Tuple<int, int>(_y, _x));
                }
            }
        }
        return list;
    }

    #endregion

    #region 서브그리드의 셀에서 한 값이 차지하는 라인 영역 반환
    public (List<int>, List<int>) GetLinesDisabledBySG(int y, int x, int value) //서브그리드 좌표 매개변수, row, col 순으로 반환, 
    {
        List<int> rows = new List<int>();
        List<int> cols = new List<int>();
        for (int _y = y * 3; _y < y * 3 + 3; _y++)
        {
            for (int _x = x * 3; _x < x * 3 + 3; _x++)
            {
                if (memoSudoku[value - 1, _y, _x] == 1)
                {
                    rows.Add(_y);
                    cols.Add(_x);
                }
            }
        }
        rows = rows.Distinct().ToList();
        rows.Sort();

        cols = cols.Distinct().ToList();
        cols.Sort();

        return (rows, cols); //끔찍해
    }

    public List<Tuple<int, int>> GetSGsDisbledByRow(int y, int value)
    {
        List<Tuple<int, int>> SGs = new List<Tuple<int, int>>();
        for (int _x = 0; _x < 9; _x++)
        {
            if (memoSudoku[value - 1, y, _x] == 1)
            {
                SGs.Add(new Tuple<int, int>(y / 3, _x / 3));
            }
        }
        SGs = SGs.Distinct().ToList();
        SGs.Sort();

        return SGs;
    }

    public List<Tuple<int, int>> GetSGsDisbledByCol(int x, int value)
    {
        List<Tuple<int, int>> SGs = new List<Tuple<int, int>>();
        for (int _y = 0; _y < 9; _y++)
        {
            if (memoSudoku[value - 1, _y, x] == 1)
            {
                SGs.Add(new Tuple<int, int>(_y / 3, x / 3));
            }
        }
        SGs = SGs.Distinct().ToList();
        SGs.Sort();

        return SGs;
    }

    #endregion

    #region 활성화된 메모값 반환
    public List<int> GetActiveMemoValue(int y, int x) //1~9
    {
        List<int> memoValueList = new List<int>();
        for (int val = 1; val <= 9; val++)
        {
            if (IsInMemoCell(y, x, val))
            {
                memoValueList.Add(val);
            }
        }
        return memoValueList;
    }
    public List<List<int>> GetMemoValuesInRow(int y)
    {
        List<List<int>> mv = new List<List<int>>();
        for (int _x = 0; _x < 9; _x++)
        {
            if (IsEmptyCell(y, _x))
            {
                mv.Add(GetActiveMemoValue(y, _x));
            }
        }
        return mv;
    }
    public List<List<int>> GetMemoValuesInCol(int x)
    {
        List<List<int>> mv = new List<List<int>>();
        for (int _y = 0; _y < 9; _y++)
        {
            if (IsEmptyCell(_y, x))
            {
                mv.Add(GetActiveMemoValue(_y, x));
            }
        }
        return mv;
    }
    public List<List<int>> GetMemoValuesInSG(int y, int x)
    {
        List<List<int>> mv = new List<List<int>>();
        for (int _y = y * 3; _y < y * 3 + 3; _y++)
        {
            for (int _x = x * 3; _x < x * 3 + 3; _x++)
            {
                if (IsEmptyCell(_y, _x))
                {
                    mv.Add(GetActiveMemoValue(_y, _x));
                }
            }
        }
        return mv;
    }
    #endregion

    #region 두 리스트/셀의 겹치는 여부/ 셀/ 값들 반환

    public int GetSameAreaCode(Tuple<int, int> c1, Tuple<int, int> c2)
    {
        if (c1.Item1 / 3 == c2.Item1 / 3 && c1.Item2 / 3 == c2.Item2 / 3)
        {
            return 2;
        }
        else if (c1.Item2 == c2.Item2)
        {
            return 1;
        }
        else if (c1.Item1 == c2.Item1)
        {
            return 0;
        }
        else
        {
            return -1;
        }
    }

    public bool IsTwoCellInSameArea(Tuple<int, int> c1, Tuple<int, int> c2)
    {
        if (c1.Item1 == c2.Item1)
        {
            return true;
        }
        else if (c1.Item2 == c2.Item2)
        {
            return true;
        }
        else if (c1.Item1 / 3 == c2.Item1 / 3 && c1.Item2 / 3 == c2.Item2 / 3)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public List<Tuple<int, int>> GetCellForcingArea(int y, int x)
    {
        List<Tuple<int, int>> list = new List<Tuple<int, int>>();
        for (int _x = 0; _x < 9; _x++)
        {
            if (IsEmptyCell(y, _x))
            {
                list.Add(new Tuple<int, int>(y, _x));
            }
        }

        for (int _y = 0; _y < 9; _y++)
        {
            if (IsEmptyCell(_y, x))
            {
                list.Add(new Tuple<int, int>(_y, x));
            }
        }

        //7,5
        int sgy = y / 3;
        int sgx = x / 3;
        for (int _y = sgy * 3; _y < sgy * 3 + 3; _y++)
        {
            for (int _x = sgx * 3; _x < sgx * 3 + 3; _x++)
            {
                if (IsEmptyCell(_y, _x))
                {
                    list.Add(new Tuple<int, int>(_y, _x));
                }
            }
        }
        list = list.Distinct().ToList();
        list.Sort();

        list.Remove(new Tuple<int, int>(y, x));

        return list;
    }

    public List<Tuple<int, int>> GetDuplicatedCellByTwoCell(Tuple<int, int> c1, Tuple<int, int> c2)
    {
        List<Tuple<int, int>> list = new List<Tuple<int, int>>();
        var cfa1 = GetCellForcingArea(c1.Item1, c1.Item2);
        var cfa2 = GetCellForcingArea(c2.Item1, c2.Item2);

        foreach (var c in cfa1)
        {
            if (cfa2.Contains(c) && c != c1 && c != c2)
            {
                list.Add(c);
            }
        }

        return list;
    }

    public List<int> GetDuplicatedValueByTwoCell(Tuple<int, int> c1, Tuple<int, int> c2)
    {
        List<int> list = new List<int>();

        var amv1 = GetActiveMemoValue(c1.Item1, c1.Item2);
        var amv2 = GetActiveMemoValue(c2.Item1, c2.Item2);

        foreach (var amv in amv1)
        {
            if (amv2.Contains(amv))
            {
                list.Add(amv);
            }
        }

        return list;
    }

    public List<Tuple<int, int>> GetDuplicatedCellsByTwoList(List<Tuple<int, int>> l1, List<Tuple<int, int>> l2)
    {
        List<Tuple<int, int>> list = new List<Tuple<int, int>>();
        foreach (var tuple in l1)
        {
            if (l2.Contains(tuple))
            {
                list.Add(tuple);
            }
        }
        return list;
    }
    #endregion

    #region 링크된 셀들 반환
    public List<Tuple<int, int>> GetLinkedCell(int y, int x, int value, int preCode)
    {
        List<Tuple<int, int>> link_list = new List<Tuple<int, int>>();

        //link_list[0] -> row, [1] -> col, [2] -> sg
        var mcr = GetMemoCellInRow(y, value);
        if (mcr.Count == 2 && preCode != 0)
        {
            Tuple<int, int> tuple = new Tuple<int, int>(y, (mcr[0] == x ? mcr[1] : mcr[0]));
            link_list.Add(tuple);
        }
        else
        {
            link_list.Add(null);
        }

        var mcc = GetMemoCellInCol(x, value);
        if (mcc.Count == 2 && preCode != 1)
        {
            Tuple<int, int> tuple = new Tuple<int, int>((mcc[0] == y ? mcc[1] : mcc[0]), x);
            link_list.Add(tuple);
        }
        else
        {
            link_list.Add(null);
        }

        var mcsg = GetMemoCellInSG(y / 3, x / 3, value);
        if (mcsg.Count == 2 && preCode != 2)
        {
            Tuple<int, int> tuple;

            if (mcsg[0].Item1 == y && mcsg[0].Item2 == x)
            {
                tuple = mcsg[1];
            }
            else
            {
                tuple = mcsg[0];
            }

            foreach (var l in link_list)
            {
                if (l != null && l.Equals(tuple))
                {
                    tuple = null;
                    break;
                }
            }
            link_list.Add(tuple);
        }
        else
        {
            link_list.Add(null);
        }

        return link_list;
    }

    public Tuple<int, int> GetLinkedCellRecursive(int y, int x, int value, int now_cnt, int ex_cnt, int preCode)
    {
        if (now_cnt == ex_cnt)
        {
            var t = new Tuple<int, int>(y, x);

            tracer = new List<Tuple<int, int>>();
            tracer.Add(t);
            //print($"y:{y},x:{x},value:{value},now_cnt:{now_cnt},ex_cnt:{ex_cnt},preCode:{preCode}");
            return t;
        }

        var link_list = GetLinkedCell(y, x, value, preCode);

        //print($"y:{y},x:{x},value:{value},now_cnt:{now_cnt},ex_cnt:{ex_cnt},preCode:{preCode}");
        for (int i = 0; i < 3; i++)
        {
            if (link_list[i] == null)
            {
                continue;
            }

            var next_tuple = GetLinkedCellRecursive(link_list[i].Item1, link_list[i].Item2, value, now_cnt + 1, ex_cnt, i);
            if (next_tuple != null)
            {
                var now = new Tuple<int, int>(y, x);
                if (!tracer.Contains(now))
                {
                    tracer.Add(now);
                    return next_tuple;
                }
                else
                {
                    return null;
                }
            }
        }

        return null;
    }

    #endregion

    #region 체인 반환

    public List<Tuple<int, int>> GetDisabledCellByNewCell(int y, int x, int value)
    {
        var cfa = GetCellForcingArea(y, x);
        List<Tuple<int, int>> list = new List<Tuple<int, int>>();
        foreach (var c in cfa)
        {
            if (IsInMemoCell(c.Item1, c.Item2, value))
            {
                list.Add(new Tuple<int, int>(c.Item1, c.Item2));
            }
        }
        return list;
    }

    public List<Tuple<int, int>> GetCellToFillByNewCell(int y, int x, int value)
    {
        List<Tuple<int, int>> list = new List<Tuple<int, int>>();
        //row
        var mcr = GetMemoCellInRow(y, value);
        if (mcr.Count == 2)
        {
            int adder = mcr[0] == x ? mcr[1] : mcr[0];
            list.Add(new Tuple<int, int>(y, adder));
        }

        var mcc = GetMemoCellInCol(x, value);
        if (mcc.Count == 2)
        {
            int adder = mcc[0] == y ? mcc[1] : mcc[0];
            list.Add(new Tuple<int, int>(adder, x));
        }

        var mcsg = GetMemoCellInSG(y / 3, x / 3, value);
        if (mcsg.Count == 2)
        {
            var adder = (mcsg[0] == new Tuple<int, int>(y, x)) ? mcsg[1] : mcsg[0];
            list.Add(new Tuple<int, int>(adder.Item1, adder.Item2));
        }

        return list;
    }

    public void GetXOChainRecursive(int y, int x, int value, int now_cnt, int ex_cnt, List<Tuple<int, int>> tonext)
    {
        var ctf_nc = GetCellToFillByNewCell(y, x, value); //1->2
        foreach (var ctf in ctf_nc)
        {
            if (tonext.Contains(ctf))
            {
                continue;
            }
            //여기서 탈출
            if (now_cnt == ex_cnt)
            {
                List<Tuple<int, int>> last = new List<Tuple<int, int>>();

                last.AddRange(tonext);
                last.Add(ctf);
                tracerList.Add(last.ToList());

                continue;
            }
            else
            {

                var dc_nc = GetDisabledCellByNewCell(ctf.Item1, ctf.Item2, value); //2->3
                foreach (var dc in dc_nc)
                {
                    if (tonext.Contains(dc) || ctf == dc)
                    {
                        continue;
                    }
                    List<Tuple<int, int>> new_tonext = new List<Tuple<int, int>>();
                    new_tonext.AddRange(tonext);
                    new_tonext.Add(ctf);
                    new_tonext.Add(dc);
                    GetXOChainRecursive(dc.Item1, dc.Item2, value, now_cnt + 1, ex_cnt, new_tonext);
                }
            }
        }
        return;
    }

    public void GetXOChainRecursiveWithTracer(int y, int x, int value, int now_cnt, int ex_cnt)
    {
        tracerList = new List<List<Tuple<int, int>>>();
        List<Tuple<int, int>> tonext = new List<Tuple<int, int>>();
        tonext.Add(new Tuple<int, int>(y, x));
        GetXOChainRecursive(y, x, value, now_cnt, ex_cnt, tonext);
    }
    #endregion

    public void GetOXChainRecursive(int y, int x, int value, int now_cnt, int ex_cnt, List<Tuple<int, int>> tonext)
    {
        var dc_nc = GetDisabledCellByNewCell(y, x, value); //O->X
        foreach (var dc in dc_nc)
        {
            if (tonext.Contains(dc)) //진입한 셀이면 통과
            {
                continue;
            }

            if (now_cnt == ex_cnt)
            {
                List<Tuple<int, int>> last = new List<Tuple<int, int>>();

                last.AddRange(tonext);
                last.Add(dc);
                tracerList.Add(last.ToList());
            }
            var ctf_nc = GetCellToFillByNewCell(dc.Item1, dc.Item2, value); //X->O

            foreach (var ctf in ctf_nc)
            {
                if (tonext.Contains(ctf) || dc.Equals(ctf))
                {
                    continue;
                }
                List<Tuple<int, int>> new_tonext = new List<Tuple<int, int>>();
                new_tonext.AddRange(tonext);
                new_tonext.Add(dc);
                new_tonext.Add(ctf);

                GetOXChainRecursive(ctf.Item1, ctf.Item2, value, now_cnt + 1, ex_cnt, new_tonext);
            }
        }
        return;
    }

    public void GetOXChainRecursiveWithTracer(int y, int x, int value, int now_cnt, int ex_cnt)
    {
        tracerList = new List<List<Tuple<int, int>>>();
        List<Tuple<int, int>> tonext = new List<Tuple<int, int>>();
        tonext.Add(new Tuple<int, int>(y, x));
        GetOXChainRecursive(y, x, value, now_cnt, ex_cnt, tonext);
    }

    #region 기타
    public void RecordSudokuLog()
    {
        cellManager.HighlightCells(0);

        Tuple<int[,], int[,,]> tuple = new Tuple<int[,], int[,,]>((int[,])sudoku.Clone(), (int[,,])memoSudoku.Clone());
        lateSudoku.Add(tuple);

        undoIndex++;
    }

    public (int[,], int[,,]) CallSudokuLog()
    {
        if (undoIndex < 0)
        {
            return (null, null);
        }
        var ret = ((int[,])lateSudoku[undoIndex].Item1.Clone(), (int[,,])lateSudoku[undoIndex].Item2.Clone());
        lateSudoku.RemoveAt(undoIndex);
        undoIndex--;
        return ret;
    }

    public int ValToY(int value)
    {
        return (value - 1) / 3;
    }

    public int ValToX(int value)
    {
        return (value - 1) % 3;
    }

    public int YXToVal(int y, int x)
    {
        return y * 3 + x;
    }

    #endregion 
}