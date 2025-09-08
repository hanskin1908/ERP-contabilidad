import React, { createContext, useContext, useState } from 'react'

type Toast = { id:number; message:string; type?: 'success'|'error'|'info' }
type Ctx = { show: (message:string, type?:Toast['type']) => void }

const ToastContext = createContext<Ctx>({ show: ()=>{} })

export function ToastProvider({ children }: { children: React.ReactNode }){
  const [toasts, setToasts] = useState<Toast[]>([])
  const show = (message:string, type:Toast['type']='info') => {
    const id = Date.now()
    setToasts(prev=> [...prev, { id, message, type }])
    setTimeout(()=> setToasts(prev=> prev.filter(t=> t.id !== id)), 3000)
  }
  return (
    <ToastContext.Provider value={{ show }}>
      {children}
      <div className="fixed bottom-4 right-4 space-y-2 z-50">
        {toasts.map(t=> (
          <div key={t.id} className={`px-4 py-2 rounded shadow text-white ${t.type==='success'?'bg-green-600':t.type==='error'?'bg-red-600':'bg-slate-800'}`}>{t.message}</div>
        ))}
      </div>
    </ToastContext.Provider>
  )
}

export function useToast(){
  return useContext(ToastContext)
}

