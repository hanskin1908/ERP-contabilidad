import { useEffect, useState } from 'react'
import { api } from '../api'

type Third = { id:number; razonSocial:string; nit:string }

export default function ThirdPicker({ value, onChange, placeholder='Buscar tercero', type }: { value?: number; onChange: (t: Third|null)=>void; placeholder?: string; type?: string }){
  const [query, setQuery] = useState('')
  const [options, setOptions] = useState<Third[]>([])
  const [selected, setSelected] = useState<Third|null>(null)

  useEffect(()=>{
    if (query.length>=2) {
      api.third.list(query, type).then(res=>{ const arr = Array.isArray(res)?res:res.value; setOptions(arr||[]) }).catch(()=> setOptions([]))
    } else setOptions([])
  },[query, type])

  useEffect(()=>{
    if (!value) { setSelected(null); return }
    api.third.get(value).then((t:any)=>{ setSelected(t); setQuery(`${t.razonSocial} (${t.nit})`) })
  },[value])

  const pick = (o: Third) => { setSelected(o); setQuery(`${o.razonSocial} (${o.nit})`); onChange(o) }

  return (
    <div className="relative">
      <div className="flex gap-2">
        <input className="input flex-1" placeholder={placeholder} value={query} onChange={e=>setQuery(e.target.value)} onKeyDown={e=>{ if (e.key==='Enter' && options.length>0){ e.preventDefault(); pick(options[0]) } }} />
        {selected && <button className="btn btn-secondary" onClick={()=>{ setSelected(null); setQuery(''); onChange(null) }}>Limpiar</button>}
      </div>
      {query.length>=2 && options.length>0 && (
        <div className="absolute z-50 left-0 right-0 mt-1 bg-white border border-slate-200 rounded-md max-h-48 overflow-auto shadow">
          {options.map(o=> (
            <div key={o.id} className="px-3 py-2 hover:bg-slate-50 cursor-pointer" onClick={()=> pick(o)}>
              {o.razonSocial} <span className="text-slate-500">({o.nit})</span>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
