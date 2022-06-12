/*
 * 2021.03.27-Code rectification and function enhancement.
 *            Huawei Device Co., Ltd. <mobile@huawei.com>
 */

#include "../StdAfx.h"
#include "UIEdit.h"

namespace DuiLib {
const int RADIX_HEXADECIMAL = 16; // 十六进制
const int TEXT_MAX_CHAR = 255; // text文本最大字符数量

class CEditWnd : public CWindowWnd {
public:
    CEditWnd();
    ~CEditWnd();

    void Init(CEditUI *pOwner);
    RECT CalPos();

    RECT GetCalPos();
    LPCTSTR GetWindowClassName() const;
    LPCTSTR GetSuperClassName() const;
    void OnFinalMessage(HWND hWnd);

    LRESULT HandleMessage(UINT uMsg, WPARAM wParam, LPARAM lParam);
    LRESULT OnKillFocus(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL &bHandled);
    LRESULT OnEditChanged(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL &bHandled);
    HBRUSH m_hBkBrush;

protected:
    CEditUI *m_pOwner;
    bool m_bInit;
    bool m_bDrawCaret;
};


CEditWnd::CEditWnd() : m_pOwner(nullptr), m_hBkBrush(nullptr), m_bInit(false), m_bDrawCaret(false) {}

CEditWnd::~CEditWnd() {}

void CEditWnd::Init(CEditUI *pOwner)
{
    if (pOwner == nullptr) {
        return;
    }
    m_pOwner = pOwner;
    if (m_pOwner == nullptr) {
        return;
    }

    RECT rcPos = CalPos();
    UINT uStyle = 0;
    if (m_pOwner->GetManager()->IsLayered()) {
        uStyle = WS_POPUP | ES_AUTOHSCROLL | WS_VISIBLE | WS_CHILD | WS_CLIPSIBLINGS;
        RECT rcWnd = { 0 };
        ::GetWindowRect(m_pOwner->GetManager()->GetPaintWindow(), &rcWnd);
        rcPos.left += rcWnd.left;
        rcPos.right += rcWnd.left;
        rcPos.top += rcWnd.top;
        rcPos.bottom += rcWnd.top;
    } else {
        uStyle = WS_CHILD | ES_AUTOHSCROLL | WS_CLIPSIBLINGS;
    }
    UINT uTextStyle = m_pOwner->GetTextStyle();
    if (uTextStyle & DT_LEFT) {
        uStyle |= ES_LEFT;
    } else if (uTextStyle & DT_CENTER) {
        uStyle |= ES_CENTER;
    } else if (uTextStyle & DT_RIGHT) {
        uStyle |= ES_RIGHT;
    }
    if (m_pOwner->IsPasswordMode()) {
        uStyle |= ES_PASSWORD;
    }
    Create(m_pOwner->GetManager()->GetPaintWindow(), nullptr, uStyle, 0, rcPos);
    HFONT hFont = nullptr;
    int iFontIndex = m_pOwner->GetFont();
    if (iFontIndex != -1) {
        hFont = m_pOwner->GetManager()->GetFont(iFontIndex);
    }
    if (hFont == nullptr) {
        hFont = m_pOwner->GetManager()->GetDefaultFontInfo()->hFont;
    }

    SetWindowFont(m_hWnd, hFont, TRUE);
    Edit_LimitText(m_hWnd, m_pOwner->GetMaxChar());
    if (m_pOwner->IsPasswordMode()) {
        Edit_SetPasswordChar(m_hWnd, m_pOwner->GetPasswordChar());
    }
    Edit_SetText(m_hWnd, m_pOwner->GetText());
    Edit_SetModify(m_hWnd, FALSE);
    SendMessage(EM_SETMARGINS, EC_LEFTMARGIN | EC_RIGHTMARGIN, MAKELPARAM(0, 0));
    Edit_Enable(m_hWnd, m_pOwner->IsEnabled() == true);
    Edit_SetReadOnly(m_hWnd, m_pOwner->IsReadOnly() == true);

    // Styls
    unsigned long styleValue = static_cast<unsigned long>(::GetWindowLong(m_hWnd, GWL_STYLE));
    unsigned int windowStyls = pOwner->GetWindowStyls();
    styleValue |= windowStyls;
    ::SetWindowLong(GetHWND(), GWL_STYLE, styleValue);
    ::ShowWindow(m_hWnd, SW_SHOWNOACTIVATE);

    int cchLen = ::GetWindowTextLength(m_hWnd);
    if (cchLen <= 0) {
        cchLen = 1;
    }
    ::SetFocus(m_hWnd);
    ::SendMessage(m_hWnd, EM_SETSEL, 0, cchLen);
    m_bInit = true;
}

RECT CEditWnd::CalPos()
{
    if (m_pOwner == nullptr) {
        return { 0 };
    }
    CDuiRect rcPos = m_pOwner->GetPos();
    RECT rcInset = CResourceManager::GetInstance()->Scale(m_pOwner->GetTextPadding());
    rcPos.left += rcInset.left;
    rcPos.top += rcInset.top;
    rcPos.right -= rcInset.right;
    rcPos.bottom -= rcInset.bottom;
    LONG lEditHeight = m_pOwner->GetManager()->GetFontInfo(m_pOwner->GetFont())->tm.tmHeight;
    if (lEditHeight < rcPos.GetHeight()) {
        rcPos.top += (rcPos.GetHeight() - lEditHeight) / 2; // 取中间值
        rcPos.bottom = rcPos.top + lEditHeight;
    }

    CControlUI *pParent = m_pOwner;
    RECT rcParent;
    while (pParent = pParent->GetParent()) {
        if (!pParent->IsVisible()) {
            rcPos.left = rcPos.top = rcPos.right = rcPos.bottom = 0;
            break;
        }
        rcParent = pParent->GetClientPos();
        if (!::IntersectRect(&rcPos, &rcPos, &rcParent)) {
            rcPos.left = rcPos.top = rcPos.right = rcPos.bottom = 0;
            break;
        }
    }

    return rcPos;
}

LPCTSTR CEditWnd::GetWindowClassName() const
{
    return _T("EditWnd");
}

LPCTSTR CEditWnd::GetSuperClassName() const
{
    return WC_EDIT;
}

void CEditWnd::OnFinalMessage(HWND hWnd)
{
    if (m_pOwner == nullptr) {
        return;
    }
    m_pOwner->Invalidate();
    // Clear reference and die
    if (m_hBkBrush != nullptr) {
        ::DeleteObject(m_hBkBrush);
    }
    if (m_pOwner->GetManager()->IsLayered()) {
        m_pOwner->GetManager()->RemovePaintChildWnd(hWnd);
    }
    m_pOwner->m_pWindow = nullptr;
    delete this;
}

LRESULT CEditWnd::HandleMessage(UINT uMsg, WPARAM wParam, LPARAM lParam)
{
    LRESULT lRes = 0;
    BOOL bHandled = FALSE;
    if (uMsg == WM_CREATE) {
        if (m_pOwner != nullptr && m_pOwner->GetManager()->IsLayered()) {
            m_pOwner->GetManager()->AddPaintChildWnd(m_hWnd);
            ::SetCoalescableTimer(m_hWnd, CARET_TIMERID, ::GetCaretBlinkTime(), nullptr, TIMERV_DEFAULT_COALESCING);
        }
	} else if (uMsg == WM_SETFOCUS) {
		if (m_pOwner != nullptr && m_pOwner->GetManager()->IsLayered()) {
			m_pOwner->GetManager()->SetFocus(nullptr);
		}
		
		ShowWindow(true);
		int nSize = GetWindowTextLength(m_hWnd);
		if (nSize == 0) {
			nSize = 1;
		}
		Edit_SetSel(m_hWnd, 0, nSize);
	} else if (uMsg == WM_KILLFOCUS) {
		ShowWindow(false);
		if (m_pOwner != nullptr) {
			m_pOwner->GetManager()->SendNotify(m_pOwner, DUI_MSGTYPE_EDIT_KILLFOCUS);
			m_pOwner->GetManager()->SetFocus(nullptr);
		}
    } else if (uMsg == OCM_COMMAND) {
        if (GET_WM_COMMAND_CMD(wParam, lParam) == EN_CHANGE) {
            lRes = OnEditChanged(uMsg, wParam, lParam, bHandled);
        } else if (GET_WM_COMMAND_CMD(wParam, lParam) == EN_UPDATE) {
            RECT rcClient;
            ::GetClientRect(m_hWnd, &rcClient);
            ::InvalidateRect(m_hWnd, &rcClient, FALSE);
        }
		bHandled = TRUE;
    } else if (uMsg == WM_KEYDOWN && TCHAR(wParam) == VK_RETURN) {
        if (m_pOwner != nullptr) {
            m_pOwner->GetManager()->SendNotify(m_pOwner, DUI_MSGTYPE_RETURN);
        }
		bHandled = TRUE;
    } else if (uMsg == WM_KEYDOWN && TCHAR(wParam) == VK_ESCAPE) {
        if (m_pOwner != nullptr) {
            m_pOwner->GetManager()->SendNotify(m_pOwner, DUI_MSGTYPE_ESCAPE);
        }
		bHandled = TRUE;
    } else if (uMsg == WM_KEYDOWN && TCHAR(wParam) == VK_TAB) {
        if (m_pOwner != nullptr) {
            m_pOwner->GetManager()->SendNotify(m_pOwner, DUI_MSGTYPE_VK_TAB);
        }
		bHandled = TRUE;
    } else if (uMsg == WM_KEYDOWN && TCHAR(wParam) == VK_BACK) {
        if (m_pOwner != nullptr) {
            m_pOwner->GetManager()->SendNotify(m_pOwner, DUI_MSGTYPE_VK_BACK);
        }
		bHandled = TRUE;
    } else if (uMsg == OCM__BASE + WM_CTLCOLOREDIT || uMsg == OCM__BASE + WM_CTLCOLORSTATIC) {
        if (m_pOwner == nullptr) {
            return NULL;
        }
        if (m_pOwner->GetNativeEditBkColor() == 0xFFFFFFFF) {
            return NULL;
        }
        ::SetBkMode((HDC)wParam, TRANSPARENT);

        DWORD dwTextColor;
        if (m_pOwner->GetNativeEditTextColor() != 0x000000) {
            dwTextColor = m_pOwner->GetNativeEditTextColor();
        } else {
            dwTextColor = m_pOwner->GetTextColor();
        }
        ::SetTextColor((HDC)wParam, RGB(GetBValue(dwTextColor), GetGValue(dwTextColor), GetRValue(dwTextColor)));
        if (m_hBkBrush == nullptr) {
            DWORD clrColor = m_pOwner->GetNativeEditBkColor();
            m_hBkBrush = ::CreateSolidBrush(RGB(GetBValue(clrColor), GetGValue(clrColor), GetRValue(clrColor)));
        }
        lRes = reinterpret_cast<LRESULT>(m_hBkBrush);
        return lRes;
    } else {
        bHandled = FALSE;
    }

    if (!bHandled) {
        return CWindowWnd::HandleMessage(uMsg, wParam, lParam);
    }
    return lRes;
}

LRESULT CEditWnd::OnKillFocus(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL &bHandled)
{
    LRESULT lRes = ::DefWindowProc(m_hWnd, uMsg, wParam, lParam);
    PostMessage(WM_CLOSE);
    return lRes;
}

LRESULT CEditWnd::OnEditChanged(UINT, WPARAM, LPARAM, BOOL &)
{
    if (!m_bInit) {
        return 0;
    }
    if (m_pOwner == nullptr) {
        return 0;
    }
    // Copy text back
    int cchLen = ::GetWindowTextLength(m_hWnd) + 1;
    size_t mallocLen = 512;
    if (cchLen > 0) {
        mallocLen = cchLen * sizeof(TCHAR);
    }
    LPTSTR pstr = static_cast<LPTSTR>(malloc(mallocLen));
    if (pstr == nullptr) {
        return 0;
    }
    SecureZeroMemory(pstr, mallocLen);
    ::GetWindowText(m_hWnd, pstr, cchLen);
    if (pstr != nullptr) {
        m_pOwner->m_sText = pstr;
    } else {
        m_pOwner->m_sText = _T("");
    }
    m_pOwner->GetManager()->SendNotify(m_pOwner, DUI_MSGTYPE_TEXTCHANGED);
    if (m_pOwner->GetManager()->IsLayered()) {
        m_pOwner->Invalidate();
    }
    free(pstr);
    return 0;
}

#define UIMSG_START_EDIT _T("start_edit")

IMPLEMENT_DUICONTROL(CEditUI)

CEditUI::CEditUI()
    : m_pWindow(nullptr),
      m_uMaxChar(TEXT_MAX_CHAR),
      m_bReadOnly(false),
      m_bPasswordMode(false),
      m_cPasswordChar(_T('*')),
      m_uButtonState(0),
      m_dwEditbkColor(0xFFFFFFFF),
      m_dwEditTextColor(0x00000000),
      m_iWindowStyls(0),
      m_dwTipValueColor(0xFFBAC0C5)
{
    // 左、上、右、下,文本边距
    const int textPadLeftMargins = 4;
    const int textPadTopMargins = 3;
    const int textPadRightMargins = 4;
    const int textPadBottomMargins = 3;
    SetTextPadding(CDuiRect(textPadLeftMargins, textPadTopMargins,
        textPadRightMargins, textPadBottomMargins));
    SetBkColor(0xFFFFFFFF);
	
    OnNotify += MakeDelegate(this, &CEditUI::OnDoNotify);
}

CEditUI::~CEditUI() {}


void CEditUI::StartEdit()
{
    GetManager()->SendNotify(this, UIMSG_START_EDIT, 0, 0, true);
}

bool CEditUI::OnDoNotify(void *p)
{
    auto noti = reinterpret_cast<TNotifyUI *>(p);
    if (noti == nullptr) {
        return false;
    }
    if (noti->sType == UIMSG_START_EDIT) {
        if (!m_pWindow) {
            m_pWindow = new (std::nothrow) CEditWnd();
            if (m_pWindow != nullptr) {
                m_pWindow->Init(this);
            }
        }
    }
    return true;
}

LPCTSTR CEditUI::GetClass() const
{
    return _T("EditUI");
}

LPVOID CEditUI::GetInterface(LPCTSTR pstrName)
{
    if (_tcsicmp(pstrName, DUI_CTR_EDIT) == 0) {
        return static_cast<CEditUI *>(this);
    }
    return CLabelUI::GetInterface(pstrName);
}

UINT CEditUI::GetControlFlags() const
{
    if (!IsEnabled())
        return CControlUI::GetControlFlags();

    return UIFLAG_SETCURSOR | UIFLAG_TABSTOP;
}

void CEditUI::DoEvent(TEventUI &event)
{
    if (!IsMouseEnabled() && event.Type > UIEVENT__MOUSEBEGIN && event.Type < UIEVENT__MOUSEEND) {
        if (m_pParent != nullptr) {
            m_pParent->DoEvent(event);
        } else {
            CLabelUI::DoEvent(event);
        }
        return;
    }

    if (event.Type == UIEVENT_SETCURSOR && IsEnabled()) {
        ::SetCursor(::LoadCursor(nullptr, MAKEINTRESOURCE(IDC_IBEAM)));
        return;
    }

    if (event.Type == UIEVENT_WINDOWSIZE) {
        if (m_pWindow != nullptr && m_pManager != nullptr) {
            m_pManager->SetFocusNeeded(this);
        }
    }
    if (event.Type == UIEVENT_SCROLLWHEEL) {
        if (m_pWindow != nullptr) {
            return;
        }
    }
    if (event.Type == UIEVENT_SETFOCUS && IsEnabled()) {
        if (m_pWindow) {
			::SetFocus(*m_pWindow);
            return;
		}
        Invalidate();
    } 
	if (event.Type == UIEVENT_BUTTONDOWN || event.Type == UIEVENT_DBLCLICK || event.Type == UIEVENT_RBUTTONDOWN) {
        if (IsEnabled()) {
            GetManager()->ReleaseCapture();
            if (IsFocused() && m_pWindow == nullptr) {
                m_pWindow = new CEditWnd();
                if (m_pWindow == nullptr) {
                    return;
                }
                m_pWindow->Init(this);

                if (PtInRect(&m_rcItem, event.ptMouse)) {
                    int nSize = GetWindowTextLength(*m_pWindow);
                    if (nSize == 0) {
                        nSize = 1;
                    }
                    Edit_SetSel(*m_pWindow, 0, nSize);
                }
            } else if (m_pWindow != nullptr) {
#if 1
				::SetFocus(*m_pWindow);
#else
                POINT pt = event.ptMouse;
                pt.x -= m_rcItem.left + m_rcTextPadding.left;
                pt.y -= m_rcItem.top + m_rcTextPadding.top;
                ::SendMessage(*m_pWindow, WM_LBUTTONDOWN, event.wParam, MAKELPARAM(pt.x, pt.y));
#endif
            }
        }
        return;
    }
    if (event.Type == UIEVENT_MOUSEMOVE) {
        return;
    }
    if (event.Type == UIEVENT_BUTTONUP) {
        return;
    }
    if (event.Type == UIEVENT_CONTEXTMENU) {
        return;
    }
    if (event.Type == UIEVENT_MOUSEENTER) {
        if (IsEnabled()) {
            m_uButtonState |= UISTATE_HOT;
            Invalidate();
        }
        return;
    }
    if (event.Type == UIEVENT_MOUSELEAVE) {
        if (IsEnabled()) {
            m_uButtonState &= ~UISTATE_HOT;
            Invalidate();
        }
        return;
    }
    CLabelUI::DoEvent(event);
}

void CEditUI::SetEnabled(bool bEnable)
{
    CControlUI::SetEnabled(bEnable);
    if (!IsEnabled()) {
        m_uButtonState = 0;
    }
	if (m_pWindow != nullptr) {
		Edit_Enable(*m_pWindow, bEnable);
	}
}

void CEditUI::DoInit()
{
	m_pWindow = new CEditWnd();
	if (m_pWindow != nullptr) {
		m_pWindow->Init(this);
	}
}

void CEditUI::SetText(LPCTSTR pstrText)
{
    const int textSplitedSize = 2; // "@UI@"剥离"@"符号后的字节长度
#ifdef SWITCH_LANGUAGE_TEST
    CDuiString text(pstrText);
    vector<DuiLib::CDuiString> valueList = text.Split(L"@UI@");
    if (valueList.size() == textSplitedSize) {
        m_sText = valueList[0];
        m_sTextAll = valueList[1];
    } else {
#endif
        m_sText = pstrText;
#ifdef SWITCH_LANGUAGE_TEST
    }
#endif
    if (m_pWindow != nullptr) {
        Edit_SetText(*m_pWindow, m_sText);
    }
    Invalidate();
}

void CEditUI::SetMaxChar(UINT uMax)
{
    m_uMaxChar = uMax;
    if (m_pWindow != nullptr) {
        Edit_LimitText(*m_pWindow, m_uMaxChar);
    }
}

UINT CEditUI::GetMaxChar()
{
    return m_uMaxChar;
}

void CEditUI::SetReadOnly(bool bReadOnly)
{
    if (m_bReadOnly == bReadOnly) {
        return;
    }

    m_bReadOnly = bReadOnly;
    if (m_pWindow != nullptr) {
        Edit_SetReadOnly(*m_pWindow, m_bReadOnly);
    }
    Invalidate();
}

bool CEditUI::IsReadOnly() const
{
    return m_bReadOnly;
}

void CEditUI::SetNumberOnly(bool bNumberOnly)
{
    if (bNumberOnly) {
        m_iWindowStyls |= ES_NUMBER;
    } else {
        m_iWindowStyls &= ~ES_NUMBER;
    }
}

bool CEditUI::IsNumberOnly() const
{
    return m_iWindowStyls & ES_NUMBER ? true : false;
}

UINT CEditUI::GetWindowStyls() const
{
    return m_iWindowStyls;
}

void CEditUI::SetPasswordMode(bool bPasswordMode)
{
    if (m_bPasswordMode == bPasswordMode) {
        return;
    }
    m_bPasswordMode = bPasswordMode;
    Invalidate();
    if (m_pWindow != nullptr) {
        unsigned long styleValue = static_cast<unsigned long>(::GetWindowLong(*m_pWindow, GWL_STYLE));
        bPasswordMode ? styleValue |= ES_PASSWORD : styleValue &= ~ES_PASSWORD;
        ::SetWindowLong(*m_pWindow, GWL_STYLE, styleValue);
    }
}

bool CEditUI::IsPasswordMode() const
{
    return m_bPasswordMode;
}

void CEditUI::SetPasswordChar(TCHAR cPasswordChar)
{
    if (m_cPasswordChar == cPasswordChar) {
        return;
    }
    m_cPasswordChar = cPasswordChar;
    if (m_pWindow != nullptr) {
        Edit_SetPasswordChar(*m_pWindow, m_cPasswordChar);
    }
    Invalidate();
}

TCHAR CEditUI::GetPasswordChar() const
{
    return m_cPasswordChar;
}

LPCTSTR CEditUI::GetNormalImage()
{
    return m_sNormalImage;
}

void CEditUI::SetNormalImage(LPCTSTR pStrImage)
{
    m_sNormalImage = pStrImage;
    Invalidate();
}

LPCTSTR CEditUI::GetHotImage()
{
    return m_sHotImage;
}

void CEditUI::SetHotImage(LPCTSTR pStrImage)
{
    m_sHotImage = pStrImage;
    Invalidate();
}

LPCTSTR CEditUI::GetFocusedImage()
{
    return m_sFocusedImage;
}

void CEditUI::SetFocusedImage(LPCTSTR pStrImage)
{
    m_sFocusedImage = pStrImage;
    Invalidate();
}

LPCTSTR CEditUI::GetDisabledImage()
{
    return m_sDisabledImage;
}

void CEditUI::SetDisabledImage(LPCTSTR pStrImage)
{
    m_sDisabledImage = pStrImage;
    Invalidate();
}

void CEditUI::SetNativeEditBkColor(DWORD dwBkColor)
{
    m_dwEditbkColor = dwBkColor;
}

DWORD CEditUI::GetNativeEditBkColor() const
{
    return m_dwEditbkColor;
}

void CEditUI::SetNativeEditTextColor(LPCTSTR pStrColor)
{
    if (pStrColor == nullptr) {
        return;
    }
    if (*pStrColor == _T('#')) {
        pStrColor = ::CharNext(pStrColor);
    }
    LPTSTR pstr = nullptr;
    DWORD clrColor = _tcstoul(pStrColor, &pstr, RADIX_HEXADECIMAL);

    m_dwEditTextColor = clrColor;
}

DWORD CEditUI::GetNativeEditTextColor() const
{
    return m_dwEditTextColor;
}

void CEditUI::SetSel(long nStartChar, long nEndChar)
{
    if (m_pWindow != nullptr) {
        Edit_SetSel(*m_pWindow, nStartChar, nEndChar);
    }
}

void CEditUI::SetSelAll()
{
    SetSel(0, -1);
}

void CEditUI::SetReplaceSel(LPCTSTR lpszReplace)
{
    if (m_pWindow != nullptr) {
        Edit_ReplaceSel(*m_pWindow, lpszReplace);
    }
}

void CEditUI::SetTipValue(LPCTSTR pStrTipValue)
{
    const int textSplitedSize = 2; // "@UI@"剥离@后的字节长度
#ifdef SWITCH_LANGUAGE_TEST
    CDuiString text(pStrTipValue);
    vector<DuiLib::CDuiString> valueList = text.Split(L"@UI@");
    if (valueList.size() == textSplitedSize) {
        m_sTipValue = valueList[0].GetData();
        m_sTipValueMap = valueList[1].GetData();
    } else {
#endif
        m_sTipValue = pStrTipValue;
#ifdef SWITCH_LANGUAGE_TEST
    }
#endif
}

LPCTSTR CEditUI::GetTipValue()
{
    if (!IsResourceText()) {
        return m_sTipValue;
    }
    return CResourceManager::GetInstance()->GetText(m_sTipValue);
}

void CEditUI::SetTipValueColor(LPCTSTR pStrColor)
{
    if (pStrColor == nullptr) {
        return;
    }
    if (*pStrColor == _T('#')) {
        pStrColor = ::CharNext(pStrColor);
    }
    LPTSTR pstr = nullptr;
    DWORD clrColor = _tcstoul(pStrColor, &pstr, RADIX_HEXADECIMAL);

    m_dwTipValueColor = clrColor;
}

DWORD CEditUI::GetTipValueColor()
{
    return m_dwTipValueColor;
}


void CEditUI::SetPos(RECT rc, bool bNeedInvalidate)
{
    CControlUI::SetPos(rc, bNeedInvalidate);
    if (m_pWindow != nullptr) {
        RECT rcPos = m_pWindow->GetCalPos();
        ::SetWindowPos(m_pWindow->GetHWND(), nullptr, rcPos.left, rcPos.top, rcPos.right - rcPos.left,
            rcPos.bottom - rcPos.top, SWP_NOZORDER | SWP_NOACTIVATE);
    }
}

void CEditUI::Move(SIZE szOffset, bool bNeedInvalidate)
{
    CControlUI::Move(szOffset, bNeedInvalidate);
    if (m_pWindow != nullptr) {
        RECT rcPos = m_pWindow->GetCalPos();
        ::SetWindowPos(m_pWindow->GetHWND(), nullptr, rcPos.left, rcPos.top, rcPos.right - rcPos.left,
            rcPos.bottom - rcPos.top, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
    }
}

void CEditUI::SetVisible(bool bVisible)
{
    CControlUI::SetVisible(bVisible);
    if (!IsVisible() && m_pManager != nullptr) {
        m_pManager->SetFocus(nullptr);
    }
}

void CEditUI::SetInternVisible(bool bVisible)
{
    if (!IsVisible() && m_pManager != nullptr) {
        m_pManager->SetFocus(nullptr);
    }
}

SIZE CEditUI::EstimateSize(SIZE szAvailable)
{
    const int fixedSize = 6;
    if (m_cxyFixed.cy == 0 && m_pManager != nullptr) {
        return CDuiSize(CResourceManager::GetInstance()->Scale(m_cxyFixed.cx),
            m_pManager->GetFontInfo(GetFont())->tm.tmHeight + fixedSize);
    }
    return CControlUI::EstimateSize(szAvailable);
}

void CEditUI::SetAttribute(LPCTSTR pstrName, LPCTSTR pstrValue)
{
    if (pstrValue == nullptr) {
        return;
    }
    if (_tcsicmp(pstrName, _T("readonly")) == 0) {
        SetReadOnly(_tcsicmp(pstrValue, _T("true")) == 0);
    } else if (_tcsicmp(pstrName, _T("numberonly")) == 0) {
        SetNumberOnly(_tcsicmp(pstrValue, _T("true")) == 0);
    } else if (_tcsicmp(pstrName, _T("maxchar")) == 0) {
        SetMaxChar(_ttoi(pstrValue));
    } else if (_tcsicmp(pstrName, _T("normalimage")) == 0) {
        SetNormalImage(pstrValue);
    } else if (_tcsicmp(pstrName, _T("hotimage")) == 0) {
        SetHotImage(pstrValue);
    } else if (_tcsicmp(pstrName, _T("focusedimage")) == 0) {
        SetFocusedImage(pstrValue);
    } else if (_tcsicmp(pstrName, _T("disabledimage")) == 0) {
        SetDisabledImage(pstrValue);
    } else if (_tcsicmp(pstrName, _T("tipvalue")) == 0) {
        SetTipValue(pstrValue);
    } else if (_tcsicmp(pstrName, _T("tipvaluecolor")) == 0) {
        SetTipValueColor(pstrValue);
    } else if (_tcsicmp(pstrName, _T("nativetextcolor")) == 0) {
        SetNativeEditTextColor(pstrValue);
    } else if (_tcsicmp(pstrName, _T("nativebkcolor")) == 0) {
        if (*pstrValue == _T('#')) {
            pstrValue = ::CharNext(pstrValue);
        }
        LPTSTR pstr = nullptr;
        DWORD clrColor = _tcstoul(pstrValue, &pstr, RADIX_HEXADECIMAL);
        SetNativeEditBkColor(clrColor);
    } else {
        CLabelUI::SetAttribute(pstrName, pstrValue);
    }
}

void CEditUI::PaintStatusImage(HDC hDC)
{
    if (IsFocused()) {
        m_uButtonState |= UISTATE_FOCUSED;
    } else {
        m_uButtonState &= ~UISTATE_FOCUSED;
    }
    if (!IsEnabled()) {
        m_uButtonState |= UISTATE_DISABLED;
    } else {
        m_uButtonState &= ~UISTATE_DISABLED;
    }

    if ((m_uButtonState & UISTATE_DISABLED) != 0) {
        if (!m_sDisabledImage.IsEmpty()) {
            if (!DrawImage(hDC, (LPCTSTR)m_sDisabledImage)) {
            } else {
                return;
            }
        }
    } else if ((m_uButtonState & UISTATE_FOCUSED) != 0) {
        if (!m_sFocusedImage.IsEmpty()) {
            if (!DrawImage(hDC, (LPCTSTR)m_sFocusedImage)) {
            } else {
                return;
            }
        }
    } else if ((m_uButtonState & UISTATE_HOT) != 0) {
        if (!m_sHotImage.IsEmpty()) {
            if (!DrawImage(hDC, (LPCTSTR)m_sHotImage)) {
            } else {
                return;
            }
        }
    }

    if (!m_sNormalImage.IsEmpty()) {
        if (!DrawImage(hDC, (LPCTSTR)m_sNormalImage)) {
        } else {
            return;
        }
    }
}

void CEditUI::PaintText(HDC hDC)
{
    DWORD mCurTextColor = m_dwTextColor;
    if (m_dwTextColor == 0 && m_pManager != nullptr) {
        mCurTextColor = m_dwTextColor = m_pManager->GetDefaultFontColor();
    }
    if (m_dwDisabledTextColor == 0 && m_pManager != nullptr) {
        m_dwDisabledTextColor = m_pManager->GetDefaultDisabledColor();
    }

    CDuiString sDrawText = GetText();
    CDuiString sTipValue = GetTipValue();
    if (sDrawText == sTipValue || sDrawText == _T("")) {
        mCurTextColor = m_dwTipValueColor;
        sDrawText = sTipValue;
    } else {
        CDuiString sTemp = sDrawText;
        if (m_bPasswordMode) {
            sDrawText.Empty();
            LPCTSTR pStr = sTemp.GetData();
            while (*pStr != _T('\0')) {
                sDrawText += m_cPasswordChar;
                pStr = ::CharNext(pStr);
            }
        }
    }

    RECT m_rcTextPadding = CLabelUI::m_rcTextPadding;
    CResourceManager::GetInstance()->Scale(&m_rcTextPadding);

    RECT rc = m_rcItem;
    rc.left += m_rcTextPadding.left;
    rc.right -= m_rcTextPadding.right;
    rc.top += m_rcTextPadding.top;
    rc.bottom -= m_rcTextPadding.bottom;
    if (m_pManager == nullptr) {
        return;
    }
    if (IsEnabled()) {
        CRenderEngine::DrawText(hDC, m_pManager, rc, sDrawText, mCurTextColor, m_iFont, DT_SINGLELINE | m_uTextStyle);
    } else {
        CRenderEngine::DrawText(hDC, m_pManager, rc, sDrawText, m_dwDisabledTextColor, m_iFont,
            DT_SINGLELINE | m_uTextStyle);
    }

#ifdef TEST_TIPS_BUILD
    ShowToolTips();
#endif
}
RECT CEditWnd::GetCalPos()
{
    RECT rcPos = CalPos();
    if (m_pOwner->GetManager()->IsLayered()) {
        RECT rcWnd = { 0 };
        ::GetWindowRect(m_pOwner->GetManager()->GetPaintWindow(), &rcWnd);
        rcPos.left += rcWnd.left;
        rcPos.right += rcWnd.left;
        rcPos.top += rcWnd.top;
        rcPos.bottom += rcWnd.top;
    }
    return rcPos;
}
}
