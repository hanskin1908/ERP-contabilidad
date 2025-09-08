import { useEffect, useState } from 'react'
import { api } from '../api'
import { useToast } from '../components/Toast'
import AccountPicker from '../components/AccountPicker'

function toCSVUrl(content: Blob, filename: string){
  const url = URL.createObjectURL(content)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  document.body.appendChild(a)
  a.click()
  a.remove()
  URL.revokeObjectURL(url)
}

export default function Reports(){
  const [from, setFrom] = useState<string>(new Date(new Date().getFullYear(),0,1).toISOString().slice(0,10))
  const [to, setTo] = useState<string>(new Date().toISOString().slice(0,10))
  const [asOf, setAsOf] = useState<string>(new Date().toISOString().slice(0,10))
  const [isData, setIsData] = useState<any>(null)
  const [bsData, setBsData] = useState<any>(null)
  const [tbData, setTbData] = useState<any[]>([])
  const [jrData, setJrData] = useState<any[]>([])
  const [jrType, setJrType] = useState<string>('')
  const [jrCategory, setJrCategory] = useState<string>('')
  const [jrThird, setJrThird] = useState<number|undefined>(undefined)
  const [lgAccount, setLgAccount] = useState<number|undefined>(undefined)
  const [lgData, setLgData] = useState<any[]>([])
  const { show } = useToast()

  const loadIS = async ()=>{
    try{ const d = await api.reports.incomeStatement(from, to); setIsData(d) }catch(e:any){ show(e.message||'Error','error') }
  }
  const loadBS = async ()=>{
    try{ const d = await api.reports.balanceSheet(asOf); setBsData(d) }catch(e:any){ show(e.message||'Error','error') }
  }
  useEffect(()=>{ loadIS(); loadBS() }, [])

  const loadTB = async ()=>{ try { const d = await api.reports.trialBalance(from, to); setTbData(d) } catch(e:any){ show(e.message||'Error','error') } }
  const exportTB = async (fmt:'csv'|'pdf')=>{
    try{
      const blob = fmt==='csv' ? await api.reports.trialBalanceCsv(from, to) : await api.reports.trialBalancePdf(from, to)
      toCSVUrl(blob, `trial-balance_${from}_${to}.${fmt}`)
    }catch(e:any){ show(e.message||'Error','error') }
  }
  const loadJR = async ()=>{ try { const d = await api.reports.journal(from, to, jrType||undefined, jrThird, jrCategory||undefined); setJrData(d) } catch(e:any){ show(e.message||'Error','error') } }
  const exportJR = async (fmt:'csv'|'pdf')=>{
    try{
      const blob = fmt==='csv' ? await api.reports.journalCsv(from, to, jrType||undefined, jrThird, jrCategory||undefined) : await api.reports.journalPdf(from, to, jrType||undefined, jrThird, jrCategory||undefined)
      toCSVUrl(blob, `journal_${from}_${to}.${fmt}`)
    }catch(e:any){ show(e.message||'Error','error') }
  }
  const loadLG = async ()=>{ if (!lgAccount) { show('Seleccione cuenta','info'); return } try { const d = await api.reports.ledger(lgAccount, from, to); setLgData(d) } catch(e:any){ show(e.message||'Error','error') } }
  const exportLG = async (fmt:'csv'|'pdf')=>{ if (!lgAccount) return; try { const blob = fmt==='csv' ? await api.reports.ledgerCsv(lgAccount, from, to) : await api.reports.ledgerPdf(lgAccount, from, to); toCSVUrl(blob, `ledger_${lgAccount}_${from}_${to}.${fmt}`) } catch(e:any){ show(e.message||'Error','error') } }

  const exportIS = async ()=>{
    try{
      const blob = await api.reports.incomeStatementCsv(from, to)
      toCSVUrl(blob, `income-statement_${from}_${to}.csv`)
    }catch(e:any){ show(e.message||'Error','error') }
  }
  const exportBS = async ()=>{
    try{
      const blob = await api.reports.balanceSheetCsv(asOf)
      toCSVUrl(blob, `balance-sheet_${asOf}.csv`)
    }catch(e:any){ show(e.message||'Error','error') }
  }

  return (
    <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
      <div className="card">
        <div className="card-header flex items-center justify-between">
          <span>Estado de Resultados</span>
          <div className="flex items-center gap-2">
            <button className="btn btn-secondary" onClick={loadIS}>Actualizar</button>
            <button className="btn btn-primary" onClick={exportIS}>Exportar CSV</button>
          </div>
        </div>
        <div className="card-body space-y-3">
          <div className="grid grid-cols-3 gap-3">
            <div><div className="label">Desde</div><input type="date" className="input" value={from} onChange={e=>setFrom(e.target.value)} /></div>
            <div><div className="label">Hasta</div><input type="date" className="input" value={to} onChange={e=>setTo(e.target.value)} /></div>
          </div>
          {isData && (
            <table className="table">
              <tbody>
                <tr><td>Ingresos</td><td className="text-right font-mono">{isData.ingresos?.toFixed(2)}</td></tr>
                <tr><td>Costos</td><td className="text-right font-mono">{isData.costos?.toFixed(2)}</td></tr>
                <tr><td>Gastos</td><td className="text-right font-mono">{isData.gastos?.toFixed(2)}</td></tr>
                <tr><td className="font-semibold">Utilidad</td><td className="text-right font-mono font-semibold">{isData.utilidad?.toFixed(2)}</td></tr>
              </tbody>
            </table>
          )}
        </div>
      </div>
      <div className="card">
        <div className="card-header flex items-center justify-between">
          <span>Balance General</span>
          <div className="flex items-center gap-2">
            <button className="btn btn-secondary" onClick={loadBS}>Actualizar</button>
            <button className="btn btn-primary" onClick={exportBS}>Exportar CSV</button>
          </div>
        </div>
        <div className="card-body space-y-3">
          <div className="grid grid-cols-3 gap-3">
            <div><div className="label">A la fecha</div><input type="date" className="input" value={asOf} onChange={e=>setAsOf(e.target.value)} /></div>
          </div>
          {bsData && (
            <table className="table">
              <tbody>
                <tr><td>Activos</td><td className="text-right font-mono">{bsData.activos?.toFixed(2)}</td></tr>
                <tr><td>Pasivos</td><td className="text-right font-mono">{bsData.pasivos?.toFixed(2)}</td></tr>
                <tr><td className="font-semibold">Patrimonio</td><td className="text-right font-mono font-semibold">{bsData.patrimonio?.toFixed(2)}</td></tr>
              </tbody>
            </table>
          )}
        </div>
      </div>
      <div className="card">
        <div className="card-header flex items-center justify-between">
          <span>Balance de Comprobación</span>
          <div className="flex items-center gap-2">
            <button className="btn btn-secondary" onClick={loadTB}>Actualizar</button>
            <button className="btn btn-secondary" onClick={()=> exportTB('csv')}>CSV</button>
            <button className="btn btn-primary" onClick={()=> exportTB('pdf')}>PDF</button>
          </div>
        </div>
        <div className="card-body space-y-3">
          <div className="grid grid-cols-3 gap-3">
            <div><div className="label">Desde</div><input type="date" className="input" value={from} onChange={e=>setFrom(e.target.value)} /></div>
            <div><div className="label">Hasta</div><input type="date" className="input" value={to} onChange={e=>setTo(e.target.value)} /></div>
          </div>
          <div className="overflow-auto">
            <table className="table">
              <thead><tr><th>Cuenta</th><th>Nombre</th><th className="text-right">Débitos</th><th className="text-right">Créditos</th><th className="text-right">Saldo</th></tr></thead>
              <tbody>
                {tbData.map((r:any)=>(
                  <tr key={r.accountCode}><td className="font-mono">{r.accountCode}</td><td>{r.accountName}</td><td className="text-right">{r.debits?.toFixed(2)}</td><td className="text-right">{r.credits?.toFixed(2)}</td><td className="text-right">{r.balance?.toFixed(2)}</td></tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
      <div className="card xl:col-span-2">
        <div className="card-header flex items-center justify-between">
          <span>Libro Diario</span>
          <div className="flex items-center gap-2">
            <button className="btn btn-secondary" onClick={loadJR}>Actualizar</button>
            <button className="btn btn-secondary" onClick={()=> exportJR('csv')}>CSV</button>
            <button className="btn btn-primary" onClick={()=> exportJR('pdf')}>PDF</button>
          </div>
        </div>
        <div className="card-body space-y-3">
          <div className="grid grid-cols-4 gap-3">
            <div><div className="label">Desde</div><input type="date" className="input" value={from} onChange={e=>setFrom(e.target.value)} /></div>
            <div><div className="label">Hasta</div><input type="date" className="input" value={to} onChange={e=>setTo(e.target.value)} /></div>
            <div><div className="label">Tipo</div><select className="input" value={jrType} onChange={e=>setJrType(e.target.value)}><option value="">Todos</option><option value="DIARIO">DIARIO</option><option value="INGRESO">INGRESO</option><option value="EGRESO">EGRESO</option><option value="AJUSTE">AJUSTE</option></select></div>
            <div><div className="label">Categoría</div><input className="input" value={jrCategory} onChange={e=>setJrCategory(e.target.value)} placeholder="ventas, compras..." /></div>
          </div>
          <div className="overflow-auto">
            <table className="table">
              <thead><tr><th>Fecha</th><th>#</th><th>Tipo</th><th>Cuenta</th><th>Nombre</th><th>Detalle</th><th>Cat.</th><th>Tercero</th><th className="text-right">Débito</th><th className="text-right">Crédito</th></tr></thead>
              <tbody>
                {jrData.map((r:any, idx:number)=>(
                  <tr key={idx}><td>{r.date}</td><td className="font-mono">{r.number}</td><td>{r.type}</td><td className="font-mono">{r.accountCode}</td><td>{r.accountName}</td><td>{r.description}</td><td>{r.category}</td><td>{r.thirdName}</td><td className="text-right">{r.debit?.toFixed(2)}</td><td className="text-right">{r.credit?.toFixed(2)}</td></tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
      <div className="card xl:col-span-2">
        <div className="card-header flex items-center justify-between">
          <span>Libro Mayor</span>
          <div className="flex items-center gap-2">
            <button className="btn btn-secondary" onClick={loadLG}>Actualizar</button>
            <button className="btn btn-secondary" onClick={()=> exportLG('csv')}>CSV</button>
            <button className="btn btn-primary" onClick={()=> exportLG('pdf')}>PDF</button>
          </div>
        </div>
        <div className="card-body space-y-3">
          <div className="grid grid-cols-4 gap-3 items-end">
            <div className="col-span-2"><div className="label">Cuenta</div><AccountPicker value={lgAccount} onChange={a=> setLgAccount(a?.id)} /></div>
            <div><div className="label">Desde</div><input type="date" className="input" value={from} onChange={e=>setFrom(e.target.value)} /></div>
            <div><div className="label">Hasta</div><input type="date" className="input" value={to} onChange={e=>setTo(e.target.value)} /></div>
          </div>
          <div className="overflow-auto">
            <table className="table">
              <thead><tr><th>Fecha</th><th>#</th><th>Tipo</th><th>Cuenta</th><th>Nombre</th><th>Detalle</th><th>Cat.</th><th>Tercero</th><th className="text-right">Débito</th><th className="text-right">Crédito</th><th className="text-right">Saldo</th></tr></thead>
              <tbody>
                {lgData.map((r:any, idx:number)=>(
                  <tr key={idx}><td>{r.date}</td><td className="font-mono">{r.number}</td><td>{r.type}</td><td className="font-mono">{r.accountCode}</td><td>{r.accountName}</td><td>{r.description}</td><td>{r.category}</td><td>{r.thirdName}</td><td className="text-right">{r.debit?.toFixed(2)}</td><td className="text-right">{r.credit?.toFixed(2)}</td><td className="text-right font-mono">{r.runningBalance?.toFixed(2)}</td></tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  )
}
