module SpaceInvaders.Image

type Position =
    {
        X: float;
        Y: float;
    }

type Image<'ImageElement> = 
    {
        Image: 'ImageElement;
        Position: Position;
    }