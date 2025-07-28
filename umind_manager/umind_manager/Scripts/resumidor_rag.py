import os
import sys
import fitz  # PyMuPDF
from langchain_community.embeddings import HuggingFaceEmbeddings
from langchain_community.vectorstores import FAISS
from langchain.text_splitter import RecursiveCharacterTextSplitter
from transformers import pipeline
from fpdf import FPDF
import unicodedata

# Verifica argumento do caminho
if len(sys.argv) < 2:
    raise Exception("Caminho do PDF não informado.")

caminho_pdf_entrada = sys.argv[1]
caminho_saida = "saida/resumo.pdf"
os.makedirs("saida", exist_ok=True)

print("Device set to use cpu")
modelo_embedding = HuggingFaceEmbeddings(model_name="sentence-transformers/all-MiniLM-L6-v2")
sumarizador = pipeline("summarization", model="sshleifer/distilbart-cnn-12-6")

# Etapa 1: Extrair texto do PDF
def extrair_texto_pdf(caminho):
    texto_completo = ""
    with fitz.open(caminho) as doc:
        for i, pagina in enumerate(doc):
            texto_pagina = pagina.get_text()
            texto_completo += f"\n\n--- Página {i + 1} ---\n\n{texto_pagina}"
    return texto_completo

# Etapa 2: Limpar e dividir o texto
def dividir_texto(texto):
    splitter = RecursiveCharacterTextSplitter(chunk_size=1000, chunk_overlap=100)
    return splitter.split_text(texto)

# Etapa 3: Criar vetor e RAG
def gerar_resumo_por_rag(trechos):
    db = FAISS.from_texts(trechos, modelo_embedding)
    resumos = []
    for i, trecho in enumerate(trechos):
        docs = db.similarity_search(trecho, k=3)
        contexto = " ".join([d.page_content for d in docs])
        resumo = sumarizador(contexto, max_length=300, min_length=60, do_sample=False)[0]['summary_text']
        resumos.append((i + 1, resumo))
    return resumos

# Etapa 4: Salvar resumo em PDF
def gerar_pdf_resumo(resumos):
    pdf = FPDF()
    pdf.add_page()
    pdf.set_auto_page_break(auto=True, margin=15)
    pdf.add_font("ArialUnicode", '', 'arial.ttf', uni=True)  # Certifique-se de que arial.ttf está presente
    pdf.set_font("ArialUnicode", size=12)

    for i, texto in resumos:
        texto_limpo = unicodedata.normalize('NFKD', texto).encode('latin-1', 'ignore').decode('latin-1')
        pdf.multi_cell(0, 10, f"Resumo do trecho {i}:\n{texto_limpo}\n")
        pdf.ln()
    pdf.output(caminho_saida)

# Execução
texto_pdf = extrair_texto_pdf(caminho_pdf_entrada)

if texto_pdf.count('--- Página') < 15:
    raise Exception("O livro precisa ter no mínimo 15 páginas.")

trechos = dividir_texto(texto_pdf)
resumos = gerar_resumo_por_rag(trechos)
gerar_pdf_resumo(resumos)

print("Resumo gerado com sucesso.")
