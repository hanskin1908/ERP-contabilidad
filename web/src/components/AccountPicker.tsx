import { useEffect, useState } from 'react'
import { api } from '../api'

type Account = { id:number; code:string; name:string }

export default function AccountPicker({ value, onChange, placeholder='Buscar cuenta' }: { value?: number; onChange: (a: Account|null)=>void; placeholder?: string }){
  const [query, setQuery] = useState('')
  const [options, setOptions] = useState<Account[]>([])
  const [selected, setSelected] = useState<Account|null>(null)

  useEffect(()=>{
    if (query.length>=2) {
      const q = query.toLowerCase()
      api.accounts.list(query)
        .then(res=>{
          const arr = (Array.isArray(res)?res:res?.value) || []
          // Filtro adicional por código o nombre desde el cliente (case-insensitive)
          const filtered = arr.filter((a: any)=> (a.code||'').toLowerCase().includes(q) || (a.name||'').toLowerCase().includes(q))
          setOptions(filtered)
        })
        .catch(()=> setOptions([]))
    } else setOptions([])
  },[query])

  useEffect(()=>{
    if (!value) { setSelected(null); return }
    api.accounts.get(value).then((a:any)=>{ setSelected(a); setQuery(`${a.code} - ${a.name}`) })
  },[value])

  const pick = (o: Account) => { setSelected(o); setQuery(`${o.code} - ${o.name}`); onChange(o) }

  return (
    <div className="relative">
      <div className="flex gap-2">
        <input
          className="input flex-1"
          placeholder={placeholder}
          value={query}
          onChange={e=>setQuery(e.target.value)}
          onKeyDown={e=>{ if (e.key==='Enter' && options.length>0) { e.preventDefault(); pick(options[0]) } }}
        />
        {selected && <button className="btn btn-secondary" onClick={()=>{ setSelected(null); setQuery(''); onChange(null) }}>Limpiar</button>}
      </div>
      {query.length>=2 && options.length>0 && (
        <div className="absolute z-50 left-0 right-0 mt-1 bg-white border border-slate-200 rounded-md max-h-48 overflow-auto shadow">
          {options.map(o=> (
            <div key={o.id} className="px-3 py-2 hover:bg-slate-50 cursor-pointer" onClick={()=> pick(o)}>
              <span className="font-mono">{o.code}</span> - {o.name}
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
