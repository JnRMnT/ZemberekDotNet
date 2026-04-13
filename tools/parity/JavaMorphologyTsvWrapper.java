import java.io.BufferedWriter;
import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.List;
import zemberek.morphology.TurkishMorphology;
import zemberek.morphology.analysis.SentenceAnalysis;
import zemberek.morphology.analysis.SentenceWordAnalysis;
import zemberek.morphology.analysis.SingleAnalysis;
import zemberek.morphology.analysis.WordAnalysis;

public final class JavaMorphologyTsvWrapper {

    private JavaMorphologyTsvWrapper() {
    }

    public static void main(String[] args) throws IOException {
        if (args.length < 2) {
            System.err.println("Usage: java -cp <zemberek-full.jar;wrapper-classes> JavaMorphologyTsvWrapper <input.txt> <output.tsv>");
            System.exit(1);
        }

        Path inputPath = Path.of(args[0]);
        Path outputPath = Path.of(args[1]);

        if (!Files.exists(inputPath)) {
            System.err.println("Input file does not exist: " + inputPath);
            System.exit(2);
        }

        List<String> sentences = Files.readAllLines(inputPath, StandardCharsets.UTF_8);
        TurkishMorphology morphology = TurkishMorphology.createWithDefaults();

        try (BufferedWriter writer = Files.newBufferedWriter(outputPath, StandardCharsets.UTF_8)) {
            for (int sentenceIndex = 0; sentenceIndex < sentences.size(); sentenceIndex++) {
                String sentence = sentences.get(sentenceIndex);
                if (sentence == null || sentence.isBlank()) {
                    continue;
                }

                SentenceAnalysis sentenceAnalysis = morphology.analyzeAndDisambiguate(sentence);
                for (SentenceWordAnalysis swa : sentenceAnalysis) {
                    WordAnalysis wa = swa.getWordAnalysis();
                    SingleAnalysis best = swa.getBestAnalysis();

                    int count = wa.analysisCount();
                    boolean unknown = best.isUnknown() || count == 0;
                    String bestText = unknown ? "?" : best.formatLexical();

                    writer.write(Integer.toString(sentenceIndex));
                    writer.write('\t');
                    writer.write(wa.getInput());
                    writer.write('\t');
                    writer.write(bestText);
                    writer.write('\t');
                    writer.write(Integer.toString(count));
                    writer.newLine();
                }
            }
        }

        System.out.println("TSV generated: " + outputPath.toAbsolutePath());
    }
}
