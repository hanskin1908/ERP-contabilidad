import { getToken } from './auth'

const base = import.meta.env.VITE_API_BASE ?? 'http://localhost:5000/api'

async function request(path: string, options: RequestInit = {}) {
  const headers: any = { 'Content-Type': 'application/json', ...(options.headers||{}) }
  const token = getToken()
  if (token) headers['Authorization'] = `Bearer ${token}`
  const res = await fetch(`${base}${path}`, { ...options, headers })
  if (!res.ok) throw new Error(await res.text())
  return res.json()
}

export const api = {
  login: (email: string, password: string) => request('/auth/login', { method: 'POST', body: JSON.stringify({ email, password }) }),
  register: (email: string, fullName: string, password: string, role='admin') => request('/auth/register', { method:'POST', body: JSON.stringify({ email, fullName, password, role })}),
  accounts: {
    list: (search?: string) => request(`/accounts${search?`?search=${encodeURIComponent(search)}`:''}`),
    get: (id: number) => request(`/accounts/${id}`),
    create: (data: {code:string;name:string;level:number;nature:'D'|'C';parentId?:number;isPostable:boolean}) => request('/accounts', { method:'POST', body: JSON.stringify(data)})
  },
  third: {
    list: (search?: string, type?: string) => request(`/third-parties${(search||type)?`?${[search?`search=${encodeURIComponent(search)}`:'', type?`type=${encodeURIComponent(type)}`:''].filter(Boolean).join('&')}`:''}`),
    get: (id: number) => request(`/third-parties/${id}`),
    create: (data: {nit:string; dv?:string; tipo:string; razonSocial:string; direccion?:string; email?:string; telefono?:string}) => request('/third-parties', { method:'POST', body: JSON.stringify(data)}),
    update: (id:number, data: {tipo:string; razonSocial:string; direccion?:string; email?:string; telefono?:string; isActive:boolean}) => request(`/third-parties/${id}`, { method:'PUT', body: JSON.stringify(data)}),
    activate: (id:number) => request(`/third-parties/${id}/activate`, { method:'PATCH' }),
    deactivate: (id:number) => request(`/third-parties/${id}/deactivate`, { method:'PATCH' })
  },
  journal: {
    list: (from?: string, to?: string) => request(`/journal-entries${from?`?from=${from}&to=${to}`:''}`),
    get: (id: number) => request(`/journal-entries/${id}`),
    create: (payload: any) => request('/journal-entries', { method:'POST', body: JSON.stringify(payload) }),
    update: (id:number, payload:any) => request(`/journal-entries/${id}`, { method:'PUT', body: JSON.stringify(payload)}),
    post: (id: number) => request(`/journal-entries/${id}/post`, { method:'POST' })
  },
  reports: {
    incomeStatement: (from:string, to:string) => request(`/reports/income-statement?from=${from}&to=${to}`),
    balanceSheet: (asOf:string) => request(`/reports/balance-sheet?asOf=${asOf}`),
    incomeStatementCsv: async (from:string, to:string) => {
      const res = await fetch(`${base}/reports/income-statement?from=${from}&to=${to}&format=csv`, { headers: { Authorization: getToken()?`Bearer ${getToken()}`:'' } })
      if (!res.ok) throw new Error(await res.text())
      return await res.blob()
    },
    balanceSheetCsv: async (asOf:string) => {
      const res = await fetch(`${base}/reports/balance-sheet?asOf=${asOf}&format=csv`, { headers: { Authorization: getToken()?`Bearer ${getToken()}`:'' } })
      if (!res.ok) throw new Error(await res.text())
      return await res.blob()
    },
    trialBalance: (from:string, to:string) => request(`/reports/trial-balance?from=${from}&to=${to}`),
    trialBalanceCsv: async (from:string, to:string) => {
      const res = await fetch(`${base}/reports/trial-balance?from=${from}&to=${to}&format=csv`, { headers: { Authorization: getToken()?`Bearer ${getToken()}`:'' } })
      if (!res.ok) throw new Error(await res.text())
      return await res.blob()
    },
    trialBalancePdf: async (from:string, to:string) => {
      const res = await fetch(`${base}/reports/trial-balance?from=${from}&to=${to}&format=pdf`, { headers: { Authorization: getToken()?`Bearer ${getToken()}`:'' } })
      if (!res.ok) throw new Error(await res.text())
      return await res.blob()
    },
    journal: (from:string, to:string, type?:string, thirdPartyId?:number, category?:string) => request(`/reports/journal?from=${from}&to=${to}${type?`&type=${encodeURIComponent(type)}`:''}${thirdPartyId?`&thirdPartyId=${thirdPartyId}`:''}${category?`&category=${encodeURIComponent(category)}`:''}`),
    journalCsv: async (from:string, to:string, type?:string, thirdPartyId?:number, category?:string) => {
      const url = `${base}/reports/journal?from=${from}&to=${to}${type?`&type=${encodeURIComponent(type)}`:''}${thirdPartyId?`&thirdPartyId=${thirdPartyId}`:''}${category?`&category=${encodeURIComponent(category)}`:''}&format=csv`
      const res = await fetch(url, { headers: { Authorization: getToken()?`Bearer ${getToken()}`:'' } })
      if (!res.ok) throw new Error(await res.text())
      return await res.blob()
    },
    journalPdf: async (from:string, to:string, type?:string, thirdPartyId?:number, category?:string) => {
      const url = `${base}/reports/journal?from=${from}&to=${to}${type?`&type=${encodeURIComponent(type)}`:''}${thirdPartyId?`&thirdPartyId=${thirdPartyId}`:''}${category?`&category=${encodeURIComponent(category)}`:''}&format=pdf`
      const res = await fetch(url, { headers: { Authorization: getToken()?`Bearer ${getToken()}`:'' } })
      if (!res.ok) throw new Error(await res.text())
      return await res.blob()
    },
    ledger: (accountId:number, from:string, to:string) => request(`/reports/ledger?accountId=${accountId}&from=${from}&to=${to}`),
    ledgerCsv: async (accountId:number, from:string, to:string) => {
      const res = await fetch(`${base}/reports/ledger?accountId=${accountId}&from=${from}&to=${to}&format=csv`, { headers: { Authorization: getToken()?`Bearer ${getToken()}`:'' } })
      if (!res.ok) throw new Error(await res.text())
      return await res.blob()
    },
    ledgerPdf: async (accountId:number, from:string, to:string) => {
      const res = await fetch(`${base}/reports/ledger?accountId=${accountId}&from=${from}&to=${to}&format=pdf`, { headers: { Authorization: getToken()?`Bearer ${getToken()}`:'' } })
      if (!res.ok) throw new Error(await res.text())
      return await res.blob()
    }
  }
  , invoices: {
    list: (type?:string, status?:string, from?:string, to?:string) => request(`/invoices${(type||status||from||to)?`?${[type?`type=${encodeURIComponent(type)}`:'', status?`status=${encodeURIComponent(status)}`:'', from?`from=${from}`:'', to?`to=${to}`:''].filter(Boolean).join('&')}`:''}`),
    get: (id:number) => request(`/invoices/${id}`),
    create: (payload:any) => request('/invoices', { method:'POST', body: JSON.stringify(payload) }),
    update: (id:number, payload:any) => request(`/invoices/${id}`, { method:'PUT', body: JSON.stringify(payload) }),
    approve: (id:number, payload:{counterAccountId:number, postingDate?:string}) => request(`/invoices/${id}/approve`, { method:'POST', body: JSON.stringify(payload) }),
    cancel: (id:number) => request(`/invoices/${id}/cancel`, { method:'POST' })
  }
  , companyConfig: {
    get: () => request('/company-config'),
    update: (payload:any) => request('/company-config', { method:'PUT', body: JSON.stringify(payload) })
  }
}
