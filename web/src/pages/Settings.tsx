import AccountPicker from '../components/AccountPicker'
import { api } from '../api'
import { useEffect, useState } from 'react'
import { useToast } from '../components/Toast'

export default function Settings(){
  const [cfg, setCfg] = useState<any>({})
  const { show } = useToast()
  useEffect(()=>{ api.companyConfig.get().then(setCfg) }, [])
  const save = async ()=>{
    try{ await api.companyConfig.update({ cxcAccountId: cfg.cxcAccountId, cxpAccountId: cfg.cxpAccountId, ivaVentasAccountId: cfg.ivaVentasAccountId, ivaComprasAccountId: cfg.ivaComprasAccountId }); show('Configuración guardada','success') }catch(e:any){ show(e.message||'Error','error') }
  }
  return (
    <div className="card">
      <div className="card-header">Configuración de Cuentas</div>
      <div className="card-body space-y-4">
        <div className="grid grid-cols-2 gap-4">
          <div>
            <div className="label">Cuenta CxC (Ventas)</div>
            <AccountPicker value={cfg.cxcAccountId} onChange={a=> setCfg({...cfg, cxcAccountId: a?.id})} />
          </div>
          <div>
            <div className="label">Cuenta CxP (Compras)</div>
            <AccountPicker value={cfg.cxpAccountId} onChange={a=> setCfg({...cfg, cxpAccountId: a?.id})} />
          </div>
          <div>
            <div className="label">Cuenta IVA Ventas</div>
            <AccountPicker value={cfg.ivaVentasAccountId} onChange={a=> setCfg({...cfg, ivaVentasAccountId: a?.id})} />
          </div>
          <div>
            <div className="label">Cuenta IVA Compras</div>
            <AccountPicker value={cfg.ivaComprasAccountId} onChange={a=> setCfg({...cfg, ivaComprasAccountId: a?.id})} />
          </div>
        </div>
        <button className="btn btn-primary" onClick={save}>Guardar</button>
      </div>
    </div>
  )
}

