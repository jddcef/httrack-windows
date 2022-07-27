#pragma once
#include <afxwin.h>
class OurCommandLineInfo :
    public CCommandLineInfo
{
public:
	
	static BOOL m_bSkipNetwork;

	OurCommandLineInfo::OurCommandLineInfo(VOID);
	virtual void ParseParam(const TCHAR* pszParam, BOOL bFlag, BOOL bLast);
};

