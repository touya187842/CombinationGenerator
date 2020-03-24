using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace CombinationGenerator {
    public abstract class 排列組合生成器 {

        public static IFunctionAdaptor<T> 對於由<T>(變數集<T> samples, int count, 個以方法 method) {
            return new EnumerableCombinator<T>(samples, count, method);
        }
    }
    public enum 個以方法 {
        排列結合,
        組合結合
    }

    public interface IFunctionAdaptor<T> {
        ISamplingable<IList<T>> 滿足(params Func<bool>[] funcs);
    }
    public interface ISamplingable<T> {
        IFunctionAdaptor<T> 依此(int count, 個以方法 method);
        IReparseable<T> 所有();
        IReparseable<T> 滿足時();
        IReparseable<T> 不滿足時();
    }
    public interface IReparseable<T> {
        IReparseable<T> 每(int count);
        ISamplingable<T> 做(Action action);
        ISamplingable<T> 輸出字串(Action<string> action);
    }

    public class 變數集<T> : IEnumerable<T> {

        internal static T[] Var;
        public T this[int index] { get => Var[index];internal set => Var[index] = value; }
        // public T this[int index,int layer] { get => ts[index]; internal set => ts[index] = value; }
        private IList<T> ts;


        public 變數集() {
            ts = new List<T>();
        }
        public 變數集(IEnumerable<T> ts) {
            this.ts = new List<T>(ts);
        }

        public void Add(T value) => ts.Add(value);
        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < ts.Count; i++) {
                yield return ts[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator() =>GetEnumerator();
        internal void SetBuffer(int count) {
            Var = new T[count];
        }

    }

    internal class EnumerableCombinator<T> : IFunctionAdaptor<T> {

        private 變數集<T> x;
        private int count;
        private 個以方法 method;
        internal EnumerableCombinator(變數集<T> samples, int count, 個以方法 method) {
            x = samples;
            this.count = count;
            this.method = method;
            x.SetBuffer(count);
        }
        public ISamplingable<IList<T>> 滿足(params Func<bool>[] funcs) {
            return new CheckedSampleCollection<IList<T>>(check(funcs));
        }

        private IEnumerable<SampleCombination<T>> combination() {
            IEnumerator<T>[] enumerators = new IEnumerator<T>[count];
            int[] counter = new int[count];
            for (int i = 0; i < count; i++) enumerators[i] = x.GetEnumerator();
            for (int i = 1; i < count; i++) enumerators[i].MoveNext();
            while (ca(enumerators, counter)) {
                T[] temp = new T[count];
                for (int i = 0; i < count; i++) temp[i] = enumerators[i].Current;
                yield return new SampleCombination<T>(temp);
            }
        }
        private bool ca(IEnumerator<T>[] enumerators, int[] counter) { 
            for (int i = 0; i < count; i++) {
                if (enumerators[i].MoveNext()) {
                    counter[i]++;
                    for (int j = i - 1; j > -1; j--) {
                        enumerators[j].MoveNext();
                        for (int k = count - 1; k > j; k--) { 
                            for (int t = 0; t < counter[k]; t++) enumerators[j].MoveNext();
                        }
                    } 
                    return true;
                }
                enumerators[i] = x.GetEnumerator();
                counter[i] = 0;
            }
            return false;
        }
        private IEnumerable<SampleCombination<T>> permutation() {
            IEnumerator<T>[] enumerators = new IEnumerator<T>[count];
            for (int i = 0; i < count; i++) enumerators[i] = x.GetEnumerator();
            for (int i = 1; i < count; i++) enumerators[i].MoveNext();
            while (pa(enumerators)) {
                T[] temp = new T[count];
                for (int i = 0; i < count; i++) temp[i] = enumerators[i].Current;
                yield return new SampleCombination<T>(temp);
            }
        }
        private bool pa(IEnumerator<T>[] enumerators) {
            for (int i = 0; i < count; i++) {
                if (enumerators[i].MoveNext()) {
                    for (int j = i - 1; j > -1; j--) enumerators[j].MoveNext();
                    return true;
                }
                enumerators[i] = x.GetEnumerator();
            }
            return false;
        }

        private IEnumerable<SampleCombination<T>> permutation2() {
            IEnumerator<T>[] enumerators = new IEnumerator<T>[count];
            for (int i = 0; i < count; i++) enumerators[i] = x.GetEnumerator();
            for (int i = 1; i < count; i++) enumerators[i].MoveNext();

        A:
            T[] temp = new T[count];
            for (int i = 0; i < count; i++) temp[i] = enumerators[i].Current;
            yield return new SampleCombination<T>(temp);
        S:
            for (int i = 0; i < count; i++) {
                if (enumerators[i].MoveNext()) goto A;
                enumerators[i].Reset();
            }
        }

        private IEnumerable<SampleCombination<T>> choosedMethod() {
            switch (method) {
                case 個以方法.排列結合:
                    return permutation();
                case 個以方法.組合結合:
                    return combination();
                default:
                    throw new NotImplementedException();
            }
        }
        private IEnumerable<(IList<T>, bool)> check(Func<bool>[] funcs) {
            
            foreach (SampleCombination<T> sample in choosedMethod()) {
                // TODO: set varible for checking-funcs
                for (int i = 0; i < sample.Count; i++) {
                    變數集<T>.Var[i] = sample[i];
                }
                yield return (sample, funcs.Aggregate(true, (b, func) => b && func()));
                
            }
        }
    }

    internal class CheckedSampleCollection<T> : ISamplingable<T> {
        private IEnumerable<(T, bool)> ts;
        internal CheckedSampleCollection(IEnumerable<(T, bool)> samples) {
            ts = samples;
        }

        public IFunctionAdaptor<T> 依此(int count, 個以方法 method) => new EnumerableCombinator<T>(new 變數集<T>(all()), count, method);

        public IReparseable<T> 所有() => new FilteredSampleCollection<T>(every());
        public IReparseable<T> 滿足時() => new FilteredSampleCollection<T>(satisfy());
        public IReparseable<T> 不滿足時() => new FilteredSampleCollection<T>(unsatisfy());

        private IEnumerable<T> all() {
            foreach ((T sample, bool filter) in ts) {
                if (filter) yield return sample;
            }
        }
        private IEnumerable<(T, bool, bool)> every() {
            foreach ((T sample, bool filter) in ts) {
                yield return (sample, filter, true);
            }
        }
        private IEnumerable<(T, bool, bool)> satisfy() {
            foreach ((T sample, bool filter) in ts) {
                yield return (sample, filter, filter);
            }
        }
        private IEnumerable<(T, bool, bool)> unsatisfy() {
            foreach ((T sample, bool filter) in ts) {
                yield return (sample, filter, !filter);
            }
        }
    }

    internal class FilteredSampleCollection<T> : IReparseable<T> {

        private IEnumerable<(T, bool, bool)> ts;
        internal FilteredSampleCollection(IEnumerable<(T, bool, bool)> samples) {
            ts = samples;
        }

        public IReparseable<T> 每(int count) => new FilteredSampleCollection<T>(every(count));

        public ISamplingable<T> 做(Action action) {
            does(action);
            return new CheckedSampleCollection<T>(next());
        }
        public ISamplingable<T> 輸出字串(Action<string> writelineAction) {
            tostring(writelineAction);
            return new CheckedSampleCollection<T>(next());
        }

        private IEnumerable<(T, bool, bool)> every(int count) {
            int i = 0;
            foreach ((T sample, bool filter, bool flag) in ts) {
                if (flag && ++i == count) {
                    i = 0;
                    yield return (sample, filter, true);
                }
                else yield return (sample, filter, false);
            }
        }
        private void does(Action action) {
            foreach ((T sample, bool filter, bool flag) in ts) {
                if (flag) action();
            }
        }
        private void tostring(Action<string> writelineAction) {
            foreach ((T sample, bool filter, bool flag) in ts) {
                if (flag) writelineAction(sample.ToString());
            }
        }
        private IEnumerable<(T, bool)> next() {
            foreach ((T sample, bool filter, _) in ts) {
                yield return (sample, filter);
            }
        }

    }

    internal class SampleCombination<T> : IList<T>, ITypeVarierableLayer { 
        public int Count => ts.Length;
        public bool IsReadOnly => throw new NotImplementedException();

        public int Level { get; private set; }

        public T this[int index] { get => ts[index]; set => ts[index] = value; }

        private T[] ts;
        internal SampleCombination(T[] ts) {
            switch (ts[0]) {
                case ITypeVarierableLayer ts0:
                    Level = ts0.Level + 1;
                    break;
                default:
                    Level = 0;
                    break;
            }
            this.ts = ts;
        }
        public override string ToString() {
            string msg = "";
            char sp = Sperator(this);
            msg += ts[0].ToString();
            for (int i = 1; i < ts.Length; i++) {
                msg += sp;
                msg += ts[i].ToString();
            }
            return msg;
        }

        private static char Sperator(SampleCombination<T> ts) {
            switch (ts.Level) {
                case 0:
                    return ',';
                case 1:
                    return ';';
                case 2:
                    return ':';
                default:
                    return '/';
            }
        }

        public int IndexOf(T item) {
            throw new NotImplementedException();
        }
        public void Insert(int index, T item) {
            throw new NotImplementedException();
        }
        public void RemoveAt(int index) {
            throw new NotImplementedException();
        }
        public void Add(T item) {
            throw new NotImplementedException();
        }
        public void Clear() {
            throw new NotImplementedException();
        }
        public bool Contains(T item) {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public bool Remove(T item) {
            throw new NotImplementedException();
        }
        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < ts.Length; i++)
                yield return ts[i];
        }
        IEnumerator IEnumerable.GetEnumerator() => ts.GetEnumerator();
    }
    
    internal interface ITypeVarierableLayer {
        int Level { get; }
    }
   
}