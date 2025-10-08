namespace Eem.Thraxus.Common.Interfaces
{

    internal interface IInit
    {
        void Init();
    }

    internal interface IInit<in TA>
    {
        void Init(TA itemA);
    }
    
    internal interface IInit<in TA, in TB>
    {
        void Init(TA itemA, TB itemB);
    }

    internal interface IInit<in TA, in TB, in TC>
    {
        void Init(TA itemA, TB itemB, TC itemC);
    }

    internal interface IInit<in TA, in TB, in TC, in TD>
    {
        void Init(TA itemA, TB itemB, TC itemC, TD itemD);
    }

    internal interface IInit<in TA, in TB, in TC, in TD, in TE>
    {
        void Init(TA itemA, TB itemB, TC itemC, TD itemD, TE itemE);
    }

    internal interface IInit<in TA, in TB, in TC, in TD, in TE, in TF>
    {
        void Init(TA itemA, TB itemB, TC itemC, TD itemD, TE itemE, TF itemF);
    }

    internal interface IInit<in TA, in TB, in TC, in TD, in TE, in TF, in TG>
    {
        void Init(TA itemA, TB itemB, TC itemC, TD itemD, TE itemE, TF itemF, TG itemG);
    }

    internal interface IInit<in TA, in TB, in TC, in TD, in TE, in TF, in TG, in TH>
    {
        void Init(TA itemA, TB itemB, TC itemC, TD itemD, TE itemE, TF itemF, TG itemG, TH itemH);
    }

    internal interface IInit<in TA, in TB, in TC, in TD, in TE, in TF, in TG, in TH, in TI>
    {
        void Init(TA itemA, TB itemB, TC itemC, TD itemD, TE itemE, TF itemF, TG itemG, TH itemH, TI itemI);
    }

    internal interface IInit<in TA, in TB, in TC, in TD, in TE, in TF, in TG, in TH, in TI, in TJ>
    {
        void Init(TA itemA, TB itemB, TC itemC, TD itemD, TE itemE, TF itemF, TG itemG, TH itemH, TI itemI, TJ itemJ);
    }
}