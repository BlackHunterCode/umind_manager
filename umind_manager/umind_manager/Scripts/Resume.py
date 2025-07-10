# -*- coding: utf-8 -*-
import sys
from transformers import pipeline
from transformers import AutoTokenizer

def resumir_texto(texto):
    summarizer = pipeline("summarization", model="facebook/bart-large-cnn")
    tokenizer = AutoTokenizer.from_pretrained("facebook/bart-large-cnn")
    
    # Tokenizar texto para saber o tamanho
    tokens = tokenizer.encode(texto, return_tensors="pt")
    input_length = tokens.size(1)

    # Ajustar max_length para não ser maior que o texto, nem muito pequeno
    max_length = min(150, max(30, int(input_length * 0.6)))
    min_length = min(50, max_length // 2)

    resumo = summarizer(texto, max_length=max_length, min_length=min_length, do_sample=False)
    return resumo[0]['summary_text']

if __name__ == "__main__":
    texto = sys.stdin.read()
    print(resumir_texto(texto))