#include "OurCommandLineInfo.h"

BOOL OurCommandLineInfo::m_bSkipNetwork = false;
OurCommandLineInfo::OurCommandLineInfo(VOID)
{

}


void OurCommandLineInfo::ParseParam(const TCHAR* pszParam, BOOL bFlag, BOOL bLast)
{
	if (lstrcmpiA(pszParam, "skip_network") == 0 || lstrcmpiA(pszParam, "-skip_network") == 0)
		m_bSkipNetwork = true;

	CCommandLineInfo::ParseParam(pszParam, bFlag, bLast);
}
